using System;
using System.Globalization;
using System.IO;
using System.Net;
using BitMobile.Common.IO;

namespace BitMobile.IO
{
    class FtpProvider : Provider, IRemoteProvider
    {
        readonly ConnectionInfo _info;
        readonly string _root;

        static bool _usePassive;

        static DateTime _lastInvoke;

        public FtpProvider(ConnectionInfo info, string root)
        {
            _info = info;
            _root = root;

            if ((DateTime.Now - _lastInvoke).TotalSeconds > 60)
                _usePassive = false;

            _lastInvoke = DateTime.Now;

            FillItems();
        }

        public override void SaveFile(string relativePath, Stream source)
        {
            if (FileExists(relativePath))
                DeleteFile(relativePath);
            else
            {
                string path = string.Empty;
                var directoryName = Path.GetDirectoryName(relativePath);
                if (directoryName != null)
                {
                    string[] directories = directoryName.Split(Path.DirectorySeparatorChar);
                    if (directories.Length > 0)
                        foreach (string t in directories)
                        {
                            path = Path.Combine(path, t);
                            CreateDirectory(path);
                        }
                }
                else
                    throw new NullReferenceException("Incorrect relative path");
            }

            FtpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , WebRequestMethods.Ftp.UploadFile);
            using (Stream stream = request.GetRequestStream())
                source.CopyTo(stream);
            ExecuteRequest(request);

            var item = new RelativeFile {RelativePath = relativePath, Time = DateTime.UtcNow};
            Files.Add(item);
        }

        public override void DeleteFile(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            FtpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , WebRequestMethods.Ftp.DeleteFile);
            ExecuteRequest(request);

            Files.RemoveAll(val => val.RelativePath == relativePath);
        }

        public void LoadFile(string relativePath, Action<Stream> action)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            FtpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , WebRequestMethods.Ftp.DownloadFile);
            using (var response = (FtpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                action(stream);
            }
        }

        void FillItems()
        {
            FtpWebRequest request = CreateRequest(string.Format("{0}.txt", _root)
                , WebRequestMethods.Ftp.DownloadFile);

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string message = reader.ReadLine();
                        if (message != null)
                        {
                            var item = ParseItems(message);
                            Files.Add(item);
                        }
                    }
                }
            }
            catch (WebException)
            {
                if (!_usePassive)
                {
                    _usePassive = true;
                    FillItems();
                }
                else
                    throw;
            }

        }

        void CreateDirectory(string path)
        {
            if (!Files.Exists(val => val.RelativePath.StartsWith(path)))
                try
                {
                    FtpWebRequest request = CreateRequest(Path.Combine(_root, path)
                        , WebRequestMethods.Ftp.MakeDirectory);
                    ExecuteRequest(request);
                }
                catch (WebException)
                {
                }
        }

        FtpWebRequest CreateRequest(string name, string method)
        {
            string path = string.Format("{0}{1}", _info.Address, name);

            var request = (FtpWebRequest)WebRequest.Create(path);
            request.UsePassive = _usePassive;
            request.Credentials = new NetworkCredential(_info.UserName.ToLower(), _info.Password);
            request.Method = method;

            return request;
        }

        static void ExecuteRequest(FtpWebRequest request)
        {
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        public class ConnectionInfo
        {
            public string Address { get; private set; }
            public string UserName { get; private set; }
            public string Password { get; private set; }

            public ConnectionInfo(string rootPath, string userName, string password)
            {
                Address = rootPath;
                UserName = userName;
                Password = password;
            }
        }
    }
}
