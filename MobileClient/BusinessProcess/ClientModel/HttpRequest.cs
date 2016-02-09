using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class HttpRequest
    {
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        public HttpRequest()
        {
        }

        public HttpRequest(string host)
            : this()
        {
            Host = host;
        }

        [Injection]
        public IScriptEngine ScriptEngine { get; set; }

        public string Host { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Get(string query)
        {
            var req = CreateRequest(query);
            req.Method = "GET";
            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                String result;
                using (var r = new StreamReader(resp.GetResponseStream()))
                {
                    result = r.ReadToEnd();
                }
                resp.Close();
                return result;
            }
            catch (WebException e)
            {
                throw CreateException(e);
            }
        }

        public string Post(string query, string data)
        {
            var req = CreateRequest(query);
            req.Method = "POST";
            try
            {
                var s = req.GetRequestStream();
                using (var w = new StreamWriter(s, System.Text.Encoding.UTF8))
                {
                    w.Write(data);
                    w.Flush();
                }

                var resp = (HttpWebResponse)req.GetResponse();
                String result;
                using (var r = new StreamReader(resp.GetResponseStream()))
                {
                    result = r.ReadToEnd();
                }
                resp.Close();
                return result;
            }
            catch (WebException e)
            {
                throw CreateException(e);
            }
        }

        public void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        private HttpWebRequest CreateRequest(string query)
        {
            var ub = new UriBuilder(String.Format(@"{0}/{1}", Host, query));
            var request = (HttpWebRequest)System.Net.WebRequest.Create(ub.Uri);
            if (!string.IsNullOrWhiteSpace(UserName))
                request.Credentials = new NetworkCredential(UserName, Password);

            foreach (var header in _headers)
                request.Headers.Add(header.Key, header.Value);

            return request;
        }

        private Exception CreateException(WebException e)
        {
            if (ScriptEngine != null)
                return ScriptEngine.CreateException(new WebError(e));
            throw new NullReferenceException("ScriptEngine not set");
        }

        class WebError : Error
        {
            public WebError(WebException exception)
                : base("WebException", GetMassage(exception))
            {
                StatusCode = -1;
                if (exception.Response != null)
                {
                    var response = exception.Response as HttpWebResponse;
                    if (response != null)
                        StatusCode = (int)response.StatusCode;
                }
            }

            public int StatusCode { get; private set; }

            private static string GetMassage(WebException e)
            {
                if (e.Response != null)
                {
                    var response = e.Response as HttpWebResponse;
                    if (response != null)
                        return response.StatusDescription;
                }
                return e.Message;
            }
        }
    }
}
