using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BitMobile.Common.Log;

namespace BitMobile.Log
{
    public class Reporter : IReporter
    {
        private readonly IReportSender _reportSender;

        public Reporter(IReportSender reportSender)
        {
            _reportSender = reportSender;
        }
        
        public IReport CreateReport(string exeption, ReportType type)
        {
            return new Report(exeption, type);
        }

        public IReport CreateReport(Stream stream)
        {
            IReport report;
            var serializer = new XmlSerializer(typeof(Report));
            try
            {
                report = (IReport)serializer.Deserialize(stream);
            }
            catch (XmlException)
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    report = new Report("Deserialization report error", text, ReportType.Crash);
                }
            }
            return report;
        }

        public IFileSystemReport CreateFileSystemReport(string directory)
        {
            return new FileSystemReport(directory);
        }

        public Task<bool> Send(string title, string text, ReportType type)
        {
            return Send(new Report(title, text, type));
        }

        public Task<bool> Send(IReport report)
        {
            return _reportSender.SendReport(report);
        }
    }
}
