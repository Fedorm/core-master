using System;
using System.Globalization;
using System.IO;
using System.Net;
using BitMobile.Common.IO;

namespace BitMobile.IO
{
    class WebDavProvider : Provider, IRemoteProvider
    {
        readonly ConnectionInfo _info;
        readonly string _root;
        
        public WebDavProvider(ConnectionInfo info, string root)
        {
            _info = info;
            _root = root;
            
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

            HttpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , WebRequestMethods.Http.Put);
            using (Stream stream = request.GetRequestStream())
                source.CopyTo(stream);
            ExecuteRequest(request);

            var item = new RelativeFile { RelativePath = relativePath, Time = DateTime.UtcNow };
            Files.Add(item);
        }

        public override void DeleteFile(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            HttpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , @"DELETE");
            ExecuteRequest(request);

            Files.RemoveAll(val => val.RelativePath == relativePath);
        }

        public void LoadFile(string relativePath, Action<Stream> action)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            HttpWebRequest request = CreateRequest(Path.Combine(_root, relativePath)
                , WebRequestMethods.Http.Get);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                action(stream);
            }
        }

        void FillItems()
        {
            HttpWebRequest request = CreateRequest(string.Format("{0}.txt", _root)
                , WebRequestMethods.Http.Get);

                using (var response = (HttpWebResponse)request.GetResponse())
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

        void CreateDirectory(string path)
        {
            if (!Files.Exists(val => val.RelativePath.StartsWith(path)))
                try
                {
                    HttpWebRequest request = CreateRequest(Path.Combine(_root, path)
                        , WebRequestMethods.Http.MkCol);
                    ExecuteRequest(request);
                }
                catch (WebException)
                {
                }
        }

        HttpWebRequest CreateRequest(string name, string method)
        {
            string path = string.Format("{0}{1}", _info.Address, name);

            var request = (HttpWebRequest)WebRequest.Create(path);
            request.Credentials = new NetworkCredential(_info.UserName.ToLower(), _info.Password);
            request.PreAuthenticate = true;
            request.Method = method;
            request.UserAgent = "BMWebDAV";
            if (method.Equals("PUT"))
                request.Headers.Add(@"Overwrite", @"T");

            return request;
        }

        static void ExecuteRequest(HttpWebRequest request)
        {
            using (request.GetResponse())
            {
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

