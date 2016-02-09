using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BitMobile.Application;
using BitMobile.Common;
using BitMobile.Common.Log;

namespace BitMobile.Log
{
    public class Report : IReport
    {
        /// <summary>
        /// For XmlDeserialization
        /// </summary>
        /// ReSharper disable once UnusedMember.Global
        public Report()
        {
        }

        public Report(string exception, ReportType type)
            : this(PrepareTitle(exception, type), exception, type)
        {
        }

        public Report(string title, string text, ReportType type)
        {
            InitReport(title, text, type);
        }

        public string Body
        {
            get
            {
                switch (Type)
                {
                    case ReportType.Crash:
                    case ReportType.Warning:
                        return CreateBody();
                    case ReportType.Feedback:
                        return Text;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // ReSharper disable MemberCanBePrivate.Global
        public string Title { get; internal set; }
        public string Text { get; internal set; }
        public ReportType Type { get; internal set; }

        public string UserName { get; internal set; }
        public string Email { get; internal set; }

        public string Attachment { get; internal set; }
        public string[] Tags
        {
            get
            {
                return CreateTagList();
            }
        }

        internal string CurrentWorkflow { get; set; }
        internal string CurrentStep { get; set; }
        internal string CurrentScreen { get; set; }
        internal string CurrentController { get; set; }
        internal string Url { get; set; }
        internal string DeviceId { get; set; }
        internal string OsTag { get; set; }
        internal string PlatformVersionTag { get; set; }
        internal string ConfigurationNameTag { get; set; }
        internal string ConfigurationVersionTag { get; set; }
        internal string ResoureVersionTag { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public void UpdateReport(string deviceId, string osTag)
        {
            DeviceId = deviceId;
            OsTag = osTag;
        }

        public void MakeAttachment(IDictionary<string, string> nativeInfo)
        {
            Attachment = CreateAttachment(nativeInfo);
        }

        public void Serialize(Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(GetType());
                serializer.Serialize(stream, this);
            }
            catch (InvalidOperationException e)
            {
                string report = "Logger error: " + e;
                report += Environment.NewLine;
                report += Text;

                using (var writer = new StreamWriter(stream))
                    writer.Write(report);
            }
        }

        private void InitReport(string title, string text, ReportType type)
        {
            Title = title;
            Text = text;
            Type = type;
            PlatformVersionTag = CoreInformation.CoreVersion.ToString();

            var context = ApplicationContext.Current;
            if (context != null)
            {
                UserName = context.Settings.UserName;
                Url = context.Settings.BaseUrl;

                if (context.Workflow != null)
                {
                    var workflow = context.Workflow;
                    CurrentWorkflow = workflow.Name;
                    if (workflow.CurrentStep != null)
                    {
                        CurrentStep = workflow.CurrentStep.Name;

                        CurrentScreen = workflow.CurrentStep.Screen;
                        CurrentController = workflow.CurrentStep.Controller ?? workflow.Controller;
                    }
                }
                if (context.Dal != null)
                {
                    ConfigurationNameTag = context.Dal.ConfigName;
                    ConfigurationVersionTag = context.Dal.ConfigVersion;
                    ResoureVersionTag = context.Dal.ResourceVersion;

                    if (!string.IsNullOrWhiteSpace(context.Dal.UserEmail))
                        Email = context.Dal.UserEmail;
                }
            }
            Attachment = "<Empty/>";
        }

        private static string PrepareTitle(string text, ReportType type)
        {
            switch (type)
            {
                case ReportType.Crash:
                case ReportType.Warning:
                    string line = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];

                    // to remove namespace of exceptions
                    // for example: BitMobile.Utilities.Exceptions.NonFatalException ===> NonFatalException:
                    int lastDot = -1;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '.')
                            lastDot = i;
                        if (line[i] == ':')
                        {
                            if (lastDot > 0)
                                line = line.Substring(lastDot + 1);
                            break;
                        }
                    }
                    return line;
                case ReportType.Feedback:
                    throw new ArgumentException("Feedback must has title");
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private string CreateBody()
        {
            string result = string.Format(
                "Url: {1} {0}Device ID: {2} {0}Workflow: {3} {0}Step: {4} {0}Screen: {5} {0}Controller: {6} {0}"
                , Environment.NewLine, Url, DeviceId, CurrentWorkflow, CurrentStep, CurrentScreen, CurrentController);
            result += Environment.NewLine;
            result += Text;
            return result;
        }

        private string[] CreateTagList()
        {
            var tags = new List<string>(4);
            if (!string.IsNullOrWhiteSpace(OsTag))
                tags.Add(OsTag);
            if (!string.IsNullOrWhiteSpace(ConfigurationNameTag))
                tags.Add("c:" + ConfigurationNameTag);
            if (!string.IsNullOrWhiteSpace(ConfigurationVersionTag))
                tags.Add("c:" + ConfigurationVersionTag);
            if (!string.IsNullOrWhiteSpace(ResoureVersionTag))
                tags.Add(ResoureVersionTag);
            tags.Add("p:" + PlatformVersionTag);
            return tags.ToArray();
        }

        private static string CreateAttachment(IEnumerable<KeyValuePair<string, string>> nativeInfo)
        {
            var builder = new StringBuilder();
            using (var w = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                w.WriteStartDocument();
                w.WriteStartElement("Info");

                w.WriteStartElement("Settings");
                WriteProperties(ApplicationContext.Current.Settings, w);
                w.WriteEndElement();

                w.WriteStartElement("Native");
                WriteDictionary(nativeInfo, w);
                w.WriteEndElement();

                w.WriteStartElement("Log");
                WriteLog(w);
                w.WriteEndElement();

//                w.WriteStartElement("ValueStack");
//                WriteValueStack(w);
//                w.WriteEndElement();

                w.WriteEndElement();
                w.WriteEndDocument();
            }
            return builder.ToString();
        }

        private static void WriteProperties(object obj, XmlWriter writer)
        {
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = property.GetValue(obj);
                writer.WriteStartElement(property.Name);
                writer.WriteString(value != null ? value.ToString() : "null");
                writer.WriteEndElement();
            }
        }

        private static void WriteDictionary(IEnumerable<KeyValuePair<string, string>> dict, XmlWriter writer)
        {
            foreach (KeyValuePair<string, string> pair in dict)
            {
                writer.WriteStartElement(pair.Key);
                writer.WriteString(pair.Value);
                writer.WriteEndElement();
            }
        }

        private static void WriteLog(XmlWriter writer)
        {
            IEnumerable<ILog> logs = Application.Log.LogManager.Logger.GetLogs();
            foreach (ILog log in logs)
            {
                writer.WriteStartElement(log.Event);
                writer.WriteAttributeString("Date", log.Date.ToString("O"));
                if (!string.IsNullOrWhiteSpace(log.Content))
                    writer.WriteAttributeString("Content", log.Content);
                writer.WriteEndElement();
            }
        }
    }
}