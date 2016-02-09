using System.Threading.Tasks;
using BitMobile.Common.Log;

namespace BitMobile.Common.Device.Providers
{
    public interface IEmailProvider : IReportSender
    {
        Task OpenEmailManager(string[] emails, string subject, string text, string[] attachments);
    }
}