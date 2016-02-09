using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using BitMobile.Droid.Application;

namespace BitMobile.Droid
{
    public class InfobaseManager
    {
        public static InfobaseManager Current
        {
            get { return _current ?? (_current = new InfobaseManager()); }
        }

        static InfobaseManager _current;

        private static readonly string FilePath;
        private const string FileName = "infobases.xml";

        static InfobaseManager()
        {
            FilePath = Path.Combine(BitBrowserApp.RootPath, FileName);

            // to compatibility with version under 1.4.4.36
            string path = Path.Combine(
                Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
                , "FirstBit"
                , "infobases.xml");
            if (!File.Exists(FilePath) && File.Exists(path))
            {
                if (!Directory.Exists(BitBrowserApp.RootPath))
                    Directory.CreateDirectory(BitBrowserApp.RootPath);
                File.Copy(path, FilePath);
            }
        }

        readonly List<Infobase> _infobases;

        private InfobaseManager()
        {
            _infobases = Deserialize();
        }


        public Infobase[] Infobases
        {
            get
            {
                return _infobases.ToArray();
            }
        }

        static IEnumerable<Infobase> DefaultInfobases
        {
            get
            {
                var result = new List<Infobase>
                {
                    new Infobase(true)
                };
                return result;
            }
        }

        public void CreateInfobase(string name, string url, string application, string ftpPort)
        {
            if (!_infobases.Exists(val => val.Name == name))
            {
                var s = new Infobase { Name = name, BaseUrl = url, ApplicationString = application, FtpPort = ftpPort };
                _infobases.Add(s);

                Serialize(_infobases);
            }
        }

        public void SaveInfobase(string name, Settings settings)
        {
            _infobases.ForEach(val => val.IsActive = false);

            Infobase s = Infobases.FirstOrDefault(val => val.Name == name);

            if (s == null)
            {
                s = new Infobase { Name = name };
                _infobases.Add(s);
            }

            s.ApplicationString = settings.ApplicationString;
            s.BaseUrl = settings.BaseUrl;            
            s.IsActive = true;
            s.IsAutorun = true;

            Serialize(_infobases);
        }

        public void SaveInfobases()
        {
            Serialize(_infobases);
        }

        public void RemoveInfobaseAt(int index)
        {
            _infobases.RemoveAt(index);

            Serialize(_infobases);
        }

        public bool DownloadInfobases(string url)
        {
            try
            {
                WebRequest request = new HttpWebRequest(new Uri(url));
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);
                    XmlNodeList nodes = doc.DocumentElement.SelectNodes("//Products/Product");

                    var list = new List<Infobase>();
                    foreach (XmlNode node in nodes)
                    {
                        var infobase = new Infobase();
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            switch (child.Name)
                            {
                                case "Name":
                                    infobase.Name = child.InnerText;
                                    break;
                                case "Url":
                                    infobase.BaseUrl = child.InnerText;
                                    break;
                                case "App":
                                    infobase.ApplicationString = child.InnerText;
                                    break;
                            }
                        }
                        list.Add(infobase);
                    }

                    _infobases.Clear();
                    _infobases.AddRange(list);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        static void Serialize(List<Infobase> infobases)
        {
            try
            {
                if (!Directory.Exists(BitBrowserApp.RootPath))
                    Directory.CreateDirectory(BitBrowserApp.RootPath);

                using (var stream = new FileStream(FilePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(List<Infobase>));
                    serializer.Serialize(stream, infobases);
                }
            }
            catch
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                using (IsolatedStorageFileStream stream = store.OpenFile(FileName, FileMode.OpenOrCreate))
                {
                    var serializer = new XmlSerializer(typeof(List<Infobase>));
                    serializer.Serialize(stream, infobases);
                }
            }
        }

        static List<Infobase> Deserialize()
        {
            List<Infobase> result;
            try
            {
                if (File.Exists(FilePath))
                    using (var stream = new FileStream(FilePath, FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof(List<Infobase>));
                        try
                        {
                            result = (List<Infobase>)serializer.Deserialize(stream);
                        }
                        catch
                        {
                            result = new List<Infobase>();
                        }
                    }
                else
                    result = new List<Infobase>();
            }
            catch
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(FileName))
                        using (IsolatedStorageFileStream stream = store.OpenFile(FileName, FileMode.OpenOrCreate))
                        {
                            var serializer = new XmlSerializer(typeof(List<Infobase>));
                            result = (List<Infobase>)serializer.Deserialize(stream);
                        }
                    else
                        result = new List<Infobase>();
                }
            }

            if (Settings.EnableDefaultInfobases)
                foreach (var infobase in DefaultInfobases)
                    if (!result.Exists(val => val.Name == infobase.Name))
                    {
                        if (infobase.IsAutorun)
                            result.ForEach(val => val.IsAutorun = false);
                        result.Add(infobase);
                    }

            return result.OrderBy(val => val.Name).ToList();
        }

        public class Infobase
        {
            public Infobase()
                : this(false)
            {
            }

            public Infobase(bool isAutorun)
            {
                BaseUrl = Settings.DefaultUrl;

                FtpPort = Settings.DefaultFtpPort;

                var uri = new UriBuilder(BaseUrl).Uri;

                if (Settings.UseSpecialInfobaseName)
                    Name = Settings.SpecialInfobaseName;
                else
                    Name = uri.LocalPath.StartsWith("/bitmobile/")
                        ? string.Format("{0} {1}", uri.Host, uri.LocalPath.Substring("/bitmobile/".Length))
                        : uri.LocalPath.Substring(1);

                ApplicationString = Settings.DefaultApplication;
                UserName = Settings.DefaultUserName;
                Password = Settings.DefaultPassword;                
                IsActive = false;
                IsAutorun = isAutorun;
            }

            public string Name { get; set; }

            public string BaseUrl { get; set; }

            public string FtpPort { get; set; }

            public string ApplicationString { get; set; }

            public string UserName { get; set; }

            [XmlIgnore]
            public string Password { get; set; }

            public bool IsActive { get; set; }

            public bool IsAutorun { get; set; }
        }

    }
}