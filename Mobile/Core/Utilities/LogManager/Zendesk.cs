using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BitMobile.Utilities.LogManager
{
    static class Zendesk
    {
        const string URL = "https://bitmobile.zendesk.com";
		const string USER_NAME = "kvponomareva@1cbit.ru";
		const string PASSWORD = "Bitmobile555!";
        const int ATTEMPTS_NUMBER = 10;

        public static void CreateTicket(string title
            , string text
            , string requesterName
            , string requesterEmail
            , bool isUrgent
            , string[] tagList
            , string attachment)
        {
            string uploadToken = UploadFile(attachment);

            string body = "{\"ticket\":{";

            body += string.Format("\"subject\": \"{0}\",", Correct(title));

            body += Comment(text, uploadToken);

            body += string.Format("\"requester\": {{\"name\": \"{0}\", \"email\": \"{1}\"}},"
                , Correct(requesterName), Correct(requesterEmail));

            body += string.Format("\"type\": \"{0}\",", "incident");

            body += string.Format("\"priority\": \"{0}\",", isUrgent ? "urgent" : "high");

            string tags = "\"tags\": [";
            for (int i = 0; i < tagList.Length; i++)
                tags += string.Format("{0}\"{1}\"", i == 0 ? "" : ", ", Correct(tagList[i]));
            tags += "]";
            body += tags;

            body += "}}";

            POSTRequest(URL + "/api/v2/tickets.json", "application/json", body);
        }

        static string Comment(string text, string uploadToken)
        {
            text = Correct(text);
            string result = string.Format("\"comment\": {{\"body\": \"{0}\", \"uploads\": [\"{1}\"]}},"
                , text, uploadToken);

            return result;
        }

        static string UploadFile(string attachment)
        {
            string url = string.Format("{0}/api/v2/uploads.json?filename=info.xml", URL);
            string response = POSTRequest(url, "application/binary", attachment);

            response = response.Substring(10, response.Length - 10 - 1);
            var dictionary = JSONParser.ParseJson(response);
            return dictionary["token"];
        }

        static string POSTRequest(string url, string contentType, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
            request.ContentType = contentType;
            request.Accept = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            WebResponse response = request.GetResponse();
            using (Stream s = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s);
                return reader.ReadToEnd();
            }
        }

        static string Correct(string input)
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