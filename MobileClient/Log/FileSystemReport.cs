using System;
using System.IO;
using System.Net;
using System.Xml;
using BitMobile.Application;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common;
using BitMobile.Common.Log;
using BitMobile.Common.Utils;

namespace BitMobile.Log
{
    class FileSystemReport: IFileSystemReport
    {
        private readonly DateTime _startTime;
        private readonly string _directory;
        private DateTime _endTime;
        private Exception _exception;

        public FileSystemReport(string directory)
        {
            _directory = directory;
            _startTime = DateTime.UtcNow;
        }

        public int LoadedCount { get; set; }
        public long LoadedSize { get; set; }
        public int DeletedCount { get; set; }
        public long DeletedSize { get; set; }

        public void Send(Exception exception = null)
        {
            _exception = exception;
            _endTime = DateTime.UtcNow;

            try
            {
                var request = WebRequest.Create(string.Format("{0}/logWebDav", ApplicationContext.Current.Settings.Url.ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled)));
                request.Method = "POST";
                Stream stream = request.GetRequestStream();
                BuildMessageBody(stream);
                using (request.GetResponse())
                {
                }
            }
            catch (Exception e)
            {
                ApplicationContext.Current.HandleException(new NonFatalException(D.ERROR, "Send file system log error", e));
            }

        }

        private void BuildMessageBody(Stream stream)
        {
            var context = ApplicationContext.Current;

            var doc = new XmlDocument();
            XmlNode root = doc.CreateElement("Log");
            doc.AppendChild(root);

            Append(root, "UserId", context.Dal.UserId);
            Append(root, "DeviceId", context.Dal.DeviceId);
            Append(root, "StartTime", _startTime.ToString("O"));
            Append(root, "EndTime", _endTime.ToString("O"));
            Append(root, "State", (_exception == null).ToString());
            if (_exception != null)
                Append(root, "Error", _exception.ToString());
            Append(root, "Directory", _directory);
            Append(root, "LoadedSize", LoadedSize);
            Append(root, "LoadedCount", LoadedCount);
            Append(root, "DeletedSize", DeletedSize);
            Append(root, "DeletedCount", DeletedCount);
            Append(root, "ResourceVersion", DbContext.Current.Database.ResourceVersion);
            Append(root, "CoreVersion", CoreInformation.CoreVersion);
            Append(root, "ConfigName", context.Dal.ConfigName);
            Append(root, "ConfigVersion", context.Dal.ConfigVersion);
            doc.Save(stream);
        }

        private static void Append(XmlNode root, string name, object content)
        {
            XmlNode node = root.OwnerDocument.CreateElement(name);
            if (content != null)
                node.InnerText = content.ToString();
            root.AppendChild(node);
        }
    }
}