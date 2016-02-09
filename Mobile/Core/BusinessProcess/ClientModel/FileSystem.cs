using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Utilities.IO;
using BitMobile.Application;
using BitMobile.Utilities.Exceptions;
using System.Xml.Serialization;
using BitMobile.Controls;
using BitMobile.Script;
using System.Threading.Tasks;
using System.Net;
using BitMobile.Utilities;

namespace BitMobile.ClientModel
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    class FileSystem
    {
        private const string SentItems = "sent_items.txt";
        private const string FsLog = "fslog.txt";

        readonly ScriptEngine _scriptEngine;
        readonly IApplicationContext _context;

        Exception _lastException;

        // TODO: жутко небезопасно, убрать
        List<string> _sentItems;

        public FileSystem(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;

            ReadFsLog();
        }

        public string LastError
        {
            get
            {
                if (_lastException != null)
                    return _lastException.Message;
                return null;
            }
        }

        public bool SuccessSync { get; private set; }

        public DateTime LastSyncTime { get; private set; }

        public void HandleLastError()
        {
            HandleLastError("An error occured during filesystem operation");
        }

        public void HandleLastError(string message)
        {
            Exception newException;
            if (_lastException is WebException)
                newException = new ConnectionException(message, _lastException);
            else
                newException = new InputOutputException(_lastException, message);

            _context.HandleException(newException);
        }

        public void SyncShared()
        {
            SyncShared(null, null);
        }

        public void SyncShared(IJSExecutable handler)
        {
            SyncShared(handler, null);
        }

        public void SyncShared(IJSExecutable handler, object state)
        {
            InvokeAsync(SyncSharedCallback, handler, state);
        }

        public void UploadPrivate()
        {
            UploadPrivate(null, null);
        }

        public void UploadPrivate(IJSExecutable handler)
        {
            UploadPrivate(handler, null);
        }

        public void UploadPrivate(IJSExecutable handler, object state)
        {
            InvokeAsync(UploadPrivateCallback, handler, state);
        }

        public void ClearShared()
        {
            try
            {
                string path = Path.Combine(_context.LocalStorage, Provider.SharedDirectory);
                FileSystemProvider.DeleteDirectory(path);
            }
            catch (Exception e)
            {
                _context.HandleException(
                    new InputOutputException(e, "Error occured during {0} operation", "ClearShared"));
            }
        }

        public void ClearPrivate()
        {
            try
            {
                string path = Path.Combine(_context.LocalStorage, Provider.PrivateDirectory);
                FileSystemProvider.DeleteDirectory(path);

                string sentPath = Path.Combine(_context.LocalStorage, SentItems);
                if (!File.Exists(sentPath))
                    File.Delete(sentPath);
            }
            catch (Exception e)
            {
                _context.HandleException(
                    new InputOutputException(e, "Error occured during {0} operation", "ClearPrivate"));
            }
        }

        public void CreateDirectory(string name)
        {
            try
            {
                string path = FileSystemProvider.TranslatePath(_context.LocalStorage, name);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                _context.HandleException(
                    new InputOutputException(e, "Error occured during {0} operation", "CreateDirectory"));
            }
        }

        public void Delete(string name)
        {
            try
            {
                string path = FileSystemProvider.TranslatePath(_context.LocalStorage, name);

                if (File.Exists(path))
                    File.Delete(path);
                else
                    if (Directory.Exists(path))
                        FileSystemProvider.DeleteDirectory(path);
            }
            catch (Exception e)
            {
                _context.HandleException(
                    new InputOutputException(e, "Error occured during {0} operation", "Delete"));
            }
        }

        async void InvokeAsync(Action callback, IJSExecutable handler, object state)
        {
            ActionHandler.Busy = true;
            ActionHandlerEx.Busy = true;

            _lastException = null;

            try
            {
                await Task.Run(callback);
                SuccessSync = true;
            }
            catch (Exception e)
            {
                _lastException = e;
                SuccessSync = false;
            }
            finally
            {
                LastSyncTime = DateTime.Now;
                WriteFsLog();

                ActionHandler.Busy = false;
                ActionHandlerEx.Busy = false;

                if (handler != null)
                    _context.InvokeOnMainThread(() =>
                    {
                        var args = new EventArgs { State = state, Result = _lastException == null };
                        handler.ExecuteStandalone(_scriptEngine.Visitor, new object[] { args });

                        handler = null;
                        state = null;
                    });
            }
        }

        void SyncSharedCallback()
        {
            var local = new FileSystemProvider(_context.LocalStorage, Provider.SharedDirectory);
            IRemoteProvider remote = CreateRemote(Provider.SharedDirectory);

            // download
            var remoteItems = new List<Item>(remote.Items);
            foreach (Item r in remoteItems)
                if (!local.FileExists(r.RelativePath) || local.FindFile(r.RelativePath).Time < r.Time)
                {
                    Item item = r;
                    remote.LoadFile(r.RelativePath, stream => local.SaveFile(item.RelativePath, stream));
                }

            // remove deleted
            var localItems = new List<Item>(local.Items);
            foreach (Item l in localItems)
            {
                if (!remote.Items.Exists(val => val.RelativePath == l.RelativePath))
                    local.DeleteFile(l.RelativePath);
            }
        }

        void UploadPrivateCallback()
        {
            ReadSentItems();

            var local = new FileSystemProvider(_context.LocalStorage, Provider.PrivateDirectory);
            IRemoteProvider remote = CreateRemote(Provider.PrivateDirectory);

            // push
            var localItems = new List<Item>(local.Items);
            foreach (Item l in localItems)
            {
                if (!_sentItems.Contains(l.RelativePath))
                {
                    if (!remote.FileExists(l.RelativePath) 
                        || remote.FindFile(l.RelativePath).Time < l.Time 
                        || remote.FindFile(l.RelativePath).Size < l.Size) // для поддержки докачки
                    {
                        using (Stream stream = local.GetStream(l.RelativePath))
                            remote.SaveFile(l.RelativePath, stream);

                        _sentItems.Add(l.RelativePath);
                    }
                }
            }

            WriteSentItems();
        }

        IRemoteProvider CreateRemote(string root)
        {
            var uri = new Uri(_context.Settings.BaseUrl);
            string solutionName = FileSystemProvider.GetSolutionName(uri);
            if (!_context.Settings.WebDavDisabled)
            {
                string address = string.Format("{0}/webdav/", _context.Settings.BaseUrl);
                var info = new WebDAVProvider.ConnectionInfo(address, _context.DAL.UserId.ToString(), _context.Settings.Password);
                return new WebDAVProvider(info, root);
            }
            else
            {
                string userName = string.Format("{0}_{1}", solutionName, _context.DAL.UserId);
                string address = string.Format("ftp://{0}:{1}/", uri.Host, _context.Settings.FtpPort);
                var info = new FtpProvider.ConnectionInfo(address, userName, _context.Settings.Password);
                return new FtpProvider(info, root);
            }
        }

        void WriteSentItems()
        {
            string path = Path.Combine(_context.LocalStorage, SentItems);
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                var serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(stream, _sentItems);
            }

            _sentItems = null;
        }

        void ReadSentItems()
        {
            string path = Path.Combine(_context.LocalStorage, SentItems);
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(List<string>));
                    try
                    {
                        _sentItems = (List<string>)serializer.Deserialize(stream);
                    }
                    catch
                    {
                        _sentItems = new List<string>();
                    }
                }
            }
            else
                _sentItems = new List<string>();
        }


        void ReadFsLog()
        {
            SuccessSync = false;
            LastSyncTime = default(DateTime);

            string path = Path.Combine(_context.LocalStorage, FsLog);
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    string str = reader.ReadToEnd();
                    string[] split = str.Split('#');
                    try
                    {
                        LastSyncTime = DateTime.Parse(split[0]);
                        SuccessSync = bool.Parse(split[1]);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                }
            }
        }

        void WriteFsLog()
        {
            string path = Path.Combine(_context.LocalStorage, FsLog);
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                string str = "{0}#{1}".Format(LastSyncTime, SuccessSync);
                writer.Write(str);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public class EventArgs
        {
            public object State { get; set; }
            public bool Result { get; set; }
        }
    }
}