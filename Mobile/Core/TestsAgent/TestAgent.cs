using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BitMobile.TestsAgent
{
    public class TestAgent
    {
        const string locapath = "/testserver/";

        ViewProxy _viewProxy;

        public TestAgent(ViewProxy commands)
        {
            _viewProxy = commands;

            Thread thread = new Thread(WaitCommand);
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://+:8088" + locapath);
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                object result = null;
                HttpStatusCode status = HttpStatusCode.OK;
                try
                {
                    string method = request.Url.LocalPath.Remove(0, locapath.Length);
                    string query = request.Url.Query;

                    List<string> parameters = new List<string>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query.Remove(0, 1);
                        foreach (string param in query.Split('&'))
                        {
                            string value = param.Split('=')[1];
                            value = WebUtility.UrlDecode(value);
                            value = ParseUnicodeString(value);
                            parameters.Add(value);
                        }
                    }

                    if (!string.IsNullOrEmpty(method))
                        result = _viewProxy.Execute(method, parameters.ToArray());
                    else
                        result = "Error: Method name is empty";
                }
                catch (Exception e)
                {
                    result = e.Message;
                    status = HttpStatusCode.InternalServerError;
                }
                finally
                {
                    BuildResponse(response, result, status);
                }
                response.Close();
            }
        }

        string ParseUnicodeString(string s)
        {
            Dictionary<string, string> table = new Dictionary<string, string>();

            Regex r = new Regex("%u([0-9A-Fa-f]){4}");
            MatchCollection matches = r.Matches(s);
            foreach (Match m in matches)
            {
                string value = m.Value;
                if (!table.ContainsKey(value))
                {
                    short code = short.Parse(value.Remove(0, 2), NumberStyles.HexNumber);
                    table.Add(value, Encoding.Unicode.GetString(new byte[] { (byte)code, (byte)(code >> 8) }));
                }
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (var kvp in table)
            {
                sb.Replace(kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }

        void BuildResponse(HttpListenerResponse response, object content, HttpStatusCode statusCode)
        {
            MemoryStream stream = content as MemoryStream;

            try
            {
                response.StatusCode = (int)statusCode;

                if (stream != null)
                {
                    response.ContentLength64 = stream.Length;
                    Stream output = response.OutputStream;
                    stream.WriteTo(output);
                    output.Flush();
                    output.Close();
                }
                else
                {
                    string msg = content != null ? content.ToString() : "NULL";
                    if (statusCode == HttpStatusCode.OK)
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(msg);
                        response.ContentLength64 = buf.Length;
                        Stream output = response.OutputStream;
                        output.Write(buf, 0, buf.Length);
                        output.Close();
                    }
                    else
                        response.StatusDescription = msg;

                    return;
                }
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
    }
}
