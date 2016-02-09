using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BitMobile.Common.Log;

namespace BitMobile.Log
{
    public class ZendeskReportSender : IReportSender
    {
        private const int AttemptsNumber = 10;
        private const string Url = "https://bitmobile.zendesk.com";
        private const string UserName = "kvponomareva@1cbit.ru";
        private const string Token = "p3XX0gGYWk5sAPYpRjwB1dIwqFdFDfdSwb4JWuRN";

        public Task<bool> SendReport(IReport report)
        {
            Task<bool> task = Task.Factory.StartNew(() =>
            {
                int attempts = 0;
                do
                {
                    try
                    {
                        Process(report);
                        return true;
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        attempts++;
                    }
                } while (attempts < AttemptsNumber);
                return false;
            });
            return task;
        }

        private static void Process(IReport report)
        {
            string requesterName = report.UserName;
            string requesterEmail = report.Email ?? "unknown@unknown.ufo";

            string uploadToken = UploadFile(report.Attachment);

            string body = "{\"ticket\":{";

            body += string.Format("\"subject\": \"{0}\",", Correct(report.Title));

            body += Comment(report.Body, uploadToken);

            body += string.Format("\"requester\": {{\"name\": \"{0}\", \"email\": \"{1}\"}},"
                , Correct(requesterName), Correct(requesterEmail));

            body += string.Format("\"type\": \"{0}\",", report.Type == ReportType.Feedback ? "question" : "incident");

            body += string.Format("\"priority\": \"{0}\",", report.Type == ReportType.Crash ? "urgent" : "high");

            string tags = "\"tags\": [";
            for (int i = 0; i < report.Tags.Length; i++)
                tags += string.Format("{0}\"{1}\"", i == 0 ? "" : ", ", Correct(report.Tags[i]));
            tags += "]";
            body += tags;

            body += "}}";

            PostRequest(Url + "/api/v2/tickets.json", "application/json", body);
        }

        private static string Comment(string text, string uploadToken)
        {
            text = Correct(text);
            string result = string.Format("\"comment\": {{\"body\": \"{0}\", \"uploads\": [\"{1}\"]}},"
                , text, uploadToken);

            return result;
        }

        private static string UploadFile(string attachment)
        {
            string url = string.Format("{0}/api/v2/uploads.json?filename=info.xml", Url);
            string response = PostRequest(url, "application/binary", attachment);

            response = response.Substring(10, response.Length - 10 - 1);
            var dictionary = JsonParser.ParseJson(response);
            return dictionary["token"];
        }

        private static string PostRequest(string url, string contentType, string body)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Authorization"] = GetAuthHeader(UserName, Token);
            request.ContentType = contentType;
            request.Accept = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            request.SendChunked = true;
            request.TransferEncoding = Encoding.UTF8.EncodingName;

            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            WebResponse response = request.GetResponse();
            using (Stream s = response.GetResponseStream())
            {
                var reader = new StreamReader(s);
                return reader.ReadToEnd();
            }
        }

        private static string GetAuthHeader(string userName, string token)
        {
            string str = string.Format("{0}/token:{1}", userName, token);
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            return string.Format("Basic {0}", auth);
        }

        private static string Correct(string input)
        {
            return input
                .Replace('\\', '/')
                .Replace(Environment.NewLine, "\\r \\n")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace("\t", "    ")
                .Replace("\"", "\\\"");
        }
    }
}