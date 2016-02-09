using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BitMobile.Application.TestsAgent
{
    public class TestAgent
    {
        const string Locapath = "/testserver/";

        readonly ViewProxy _viewProxy;

        public TestAgent(ViewProxy commands)
        {
            _viewProxy = commands;

            var thread = new Thread(WaitCommand);
            thread.IsBackground = true;
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://+:8088" + Locapath);
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                object result = null;
                var status = HttpStatusCode.OK;
                try
                {
                    string method = request.Url.LocalPath.Remove(0, Locapath.Length);
                    string query = request.Url.Query;

                    var parameters = new List<string>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query.Remove(0, 1);
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (string param in query.Split('&'))
                        {
                            string value = param.Split('=')[1];
                            value = WebUtility.UrlDecode(value);
                            value = ParseUnicodeString(value);
                            parameters.Add(value);
                        }
                    }

                    result = !string.IsNullOrEmpty(method)
                        ? _viewProxy.Execute(method, parameters.ToArray())
                        : "Error: Method name is empty";
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
            var table = new Dictionary<string, string>();

            var r = new Regex("%u([0-9A-Fa-f]){4}");
            MatchCollection matches = r.Matches(s);
            foreach (Match m in matches)
            {
                string value = m.Value;
                if (!table.ContainsKey(value))
                {
                    short code = short.Parse(value.Remove(0, 2), NumberStyles.HexNumber);
                    table.Add(value, Encoding.Unicode.GetString(new[] { (byte)code, (byte)(code >> 8) }));
                }
            }

            var sb = new StringBuilder(s);
            foreach (var kvp in table)
            {
                sb.Replace(kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }

        void BuildResponse(HttpListenerResponse response, object content, HttpStatusCode statusCode)
        {
            var stream = content as MemoryStream;

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
