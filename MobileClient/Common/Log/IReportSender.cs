using System.Threading.Tasks;

namespace BitMobile.Common.Log
{
    public interface IReportSender
    {
        Task<bool> SendReport(IReport report);
    }
}