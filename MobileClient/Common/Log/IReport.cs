using System.Collections.Generic;
using System.IO;

namespace BitMobile.Common.Log
{
    public interface IReport
    {
        string Body { get; }
        string Title { get; }
        string Text { get; }
        ReportType Type { get; }
        string UserName { get; }
        string Email { get; }
        string Attachment { get; }
        string[] Tags { get; }
        void UpdateReport(string deviceId, string osTag);
        void MakeAttachment(IDictionary<string, string> nativeInfo);
        void Serialize(Stream stream);
    }
}
