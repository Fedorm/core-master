using System.IO;
using System.Threading.Tasks;

namespace BitMobile.Common.Log
{
    public interface IReporter
    {
        IReport CreateReport(string exeption, ReportType type);
        IReport CreateReport(Stream stream);
        IFileSystemReport CreateFileSystemReport(string directory);
        Task<bool> Send(string title, string text, ReportType type);
        Task<bool> Send(IReport report);
    }
}