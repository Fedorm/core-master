using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BitMobile.Application;
using BitMobile.Application.Controls;
using BitMobile.Application.DataAccessLayer;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using BitMobile.Common;
using BitMobile.Common.Application;
using BitMobile.Common.IO;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    class FileSystem
    {
        private const string SyncTimePrivate = "synctimeprivate.txt";
        private const string FsLog = "fslog.txt";

        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _io;

        private Exception _lastException;
        public FileSystem(IScriptEngine scriptEngine, IApplicationContext io)
        {
            _scriptEngine = scriptEngine;
            _io = io;

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

            _io.HandleException(newException);
        }

        public void SyncShared()
        {
            SyncShared(null, null);
        }

        public void SyncShared(IJsExecutable handler)
        {
            SyncShared(handler, null);
        }

        public void SyncShared(IJsExecutable handler, object state)
        {
            InvokeAsync(SyncSharedCallback, handler, state);
        }

        public void UploadPrivate()
        {
            UploadPrivate(null, null);
        }

        public void UploadPrivate(IJsExecutable handler)
        {
            UploadPrivate(handler, null);
        }

        public void UploadPrivate(IJsExecutable handler, object state)
        {
            InvokeAsync(UploadPrivateCallback, handler, state);
        }

        public void ClearShared()
        {
            try
            {
                IIOContext io = IOContext.Current;
                io.Delete(Path.Combine(_io.LocalStorage, io.SharedDirectory));
            }
            catch (Exception e)
            {
                HandleException(e, "ClearShared");
            }
        }

        public void ClearPrivate()
        {
            try
            {
                IIOContext io = IOContext.Current;
                io.Delete(Path.Combine(_io.LocalStorage, io.PrivateDirectory));
            }
            catch (Exception e)
            {
                HandleException(e, "ClearPrivate");
            }
        }

        public bool CreateDirectory(string name)
        {
            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);

                return io.CreateDirectory(path);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "CreateDirectory");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        public bool Delete(string name)
        {
            if (name.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                throw ClientException(D.UNABE_TO_DELETE_SYSTEM_DIRECTORY);

            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);

                return io.Delete(path);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "Delete");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        public bool Exists(string name)
        {
            bool result;
            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);
                result = io.Exists(path);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "Exists");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
            return result;
        }

        public void Copy(string from, string to)
        {
            try
            {
                IIOContext io = IOContext.Current;
                string source = io.TranslateLocalPath(from);
                string dest = io.TranslateLocalPath(to);

                if (!io.Exists(source))
                    throw new NonFatalException(D.FILE_NOT_EXISTS + ": " + from);
                if (io.Exists(dest))
                    throw new NonFatalException(D.FILE_ALREADY_EXISTS + ": " + to);

                io.Copy(source, dest);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + from + "; " + to);
            }
            catch (Exception e)
            {
                HandleException(e, "Exists");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        public IEnumerable<string> DirFiles(string name)
        {
            return Dir(name, true);
        }

        public IEnumerable<string> DirFolders(string name)
        {
            return Dir(name, false);
        }

        public void CreateTextFile(string name, string text)
        {
            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);
                if (!io.Exists(path))
                {
                    string dir = Path.GetDirectoryName(path);
                    io.CreateDirectory(dir);
                    using (var stream = io.FileStream(path, FileMode.CreateNew))
                    using (var writer = new StreamWriter(stream))
                        writer.Write(text);
                }
                else
                    throw new NonFatalException(D.FILE_ALREADY_EXISTS + ": " + name);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "Dir");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        public string OpenTextFile(string name)
        {
            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);
                if (io.Exists(path, FileSystemItem.File))
                {
                    using (var stream = io.FileStream(path, FileMode.Open))
                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
                throw new NonFatalException(D.FILE_NOT_EXISTS + ": " + name);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "Dir");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        private async void InvokeAsync(Action callback, IJsExecutable handler, object state)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();

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

                ControlsContext.Current.ActionHandlerLocker.Release();

                if (handler != null)
                    _io.InvokeOnMainThread(() =>
                    {
                        var args = new EventArgs { State = state, Result = _lastException == null };
                        handler.ExecuteCallback(_scriptEngine.Visitor, args, null);

                        handler = null;
                        state = null;
                    });
            }
        }

        private void SyncSharedCallback()
        {
            var report = LogManager.Reporter.CreateFileSystemReport("shared");
            try
            {
                var local = IOContext.Current.CreateLocalProvider(IOContext.Current.SharedDirectory);
                IRemoteProvider remote = IOContext.Current.CreateRemoteProvider(IOContext.Current.SharedDirectory);

                // download
                var remoteItems = new List<IFile>(remote.Files);
                foreach (IFile r in remoteItems)
                    if (!local.FileExists(r.RelativePath) || local.FindFile(r.RelativePath).Time < r.Time)
                    {
                        IFile item = r;
                        remote.LoadFile(r.RelativePath, stream => local.SaveFile(item.RelativePath, stream));

                        report.LoadedCount++;
                        report.LoadedSize += item.Size;
                    }


                // remove deleted
                var localItems = new List<IFile>(local.Files);
                foreach (IFile l in localItems)
                {
                    if (!remote.Files.Exists(val => val.RelativePath == l.RelativePath))
                    {
                        local.DeleteFile(l.RelativePath);
                        report.DeletedCount++;
                        report.DeletedSize += l.Size;
                    }
                }

                report.Send();
            }
            catch (Exception e)
            {
                report.Send(e);
                throw;
            }
        }

        private void UploadPrivateCallback()
        {
            var report = LogManager.Reporter.CreateFileSystemReport("private");
            try
            {
                DateTime syncTime = ReadSyncTime();

                var local = IOContext.Current.CreateLocalProvider(IOContext.Current.PrivateDirectory);
                IRemoteProvider remote = IOContext.Current.CreateRemoteProvider(IOContext.Current.PrivateDirectory);

                // push
                var localItems = new List<IFile>(local.Files);
                foreach (IFile l in localItems)
                {
                    if (l.Time > syncTime)
                    {
                        if (!remote.FileExists(l.RelativePath)
                            || remote.FindFile(l.RelativePath).Time < l.Time
                            || remote.FindFile(l.RelativePath).Size < l.Size) // to finish downloading after disconnect
                        {
                            using (Stream stream = local.GetStream(l.RelativePath))
                                remote.SaveFile(l.RelativePath, stream);

                            report.LoadedCount++;
                            report.LoadedSize += l.Size;
                        }
                    }
                }
                WriteSyncTime();

                report.Send();
            }
            catch (Exception e)
            {
                report.Send(e);
                throw;
            }
        }

        private void WriteSyncTime()
        {
            string path = Path.Combine(_io.LocalStorage, SyncTimePrivate);
            using (var stream = IOContext.Current.FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                writer.Write(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        }

        private DateTime ReadSyncTime()
        {
            IIOContext io = IOContext.Current;
            string path = Path.Combine(_io.LocalStorage, SyncTimePrivate);
            if (io.Exists(path))
            {
                using (var stream = io.FileStream(path, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    DateTime result;
                    if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        return result;
                }
            }
            return DateTime.MinValue;
        }

        private void ReadFsLog()
        {
            SuccessSync = false;
            LastSyncTime = default(DateTime);

            IIOContext io = IOContext.Current;

            string path = Path.Combine(_io.LocalStorage, FsLog);
            if (io.Exists(path))
            {
                using (var stream = io.FileStream(path, FileMode.Open))
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

        private void WriteFsLog()
        {
            string path = Path.Combine(_io.LocalStorage, FsLog);
            using (var stream = IOContext.Current.FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                string str = string.Format("{0}#{1}", LastSyncTime, SuccessSync);
                writer.Write(str);
            }
        }

        private IEnumerable<string> Dir(string name, bool dirFilesNotFolders)
        {
            try
            {
                IIOContext io = IOContext.Current;
                string path = io.TranslateLocalPath(name);
                if (io.Exists(path))
                {
                    IEnumerable<string> result = dirFilesNotFolders
                        ? Directory.EnumerateFiles(path)
                        : Directory.EnumerateDirectories(path);
                    return result.Select(Path.GetFileName);
                }
                throw new NonFatalException(D.DIRECTORY_NOT_EXISTS + ": " + name);
            }
            catch (CustomException e)
            {
                throw ClientException(e.FriendlyMessage);
            }
            catch (ArgumentException)
            {
                throw ClientException(D.INVALID_PATH + ": " + name);
            }
            catch (Exception e)
            {
                HandleException(e, "Dir");
                throw ClientException(D.UNEXPECTED_ERROR_OCCURED);
            }
        }

        private Exception ClientException(string message)
        {
            var error = new Error("IOException", message);
            return _scriptEngine.CreateException(error);
        }

        private void HandleException(Exception e, string operation)
        {
            _io.HandleException(new InputOutputException(e, "Error occured during {0} operation", operation));
        }

        public class EventArgs
        {
            public object State { get; set; }
            public bool Result { get; set; }
        }
    }
}