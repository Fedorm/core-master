using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BitMobile.Common.ScriptEngine;
using BitMobile.Application.Log;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class WebRequest
    {
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private IScriptEngine _scriptEngine;

        public WebRequest(IScriptEngine engine)
        {
            _scriptEngine = engine;
        }

        public string Host { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Timeout { get; set; }

        public void Get(string query)
        {
            Get(query, null, null);
        }

        public void Get(string query, IJsExecutable callback)
        {
            Get(query, callback, null);
        }

        public async void Get(string query, IJsExecutable callback, object state)
        {
            using (var req = CreateRequest())
            {
                try
                {
                    var result = await req.GetStringAsync(query);

                    if (callback != null)
                    {
                        callback.ExecuteCallback(_scriptEngine.Visitor, state, new WebRequestArgs(result));
                    }
                }
                catch (WebException e)
                {
                    HandleException(callback, state, e);
                }
                catch (TaskCanceledException e)
                {
                    HandleException(callback, state, new WebException("", WebExceptionStatus.Timeout));
                }
            }
        }

        public void Post(string query, string data)
        {
            Post(query, data, null, null);
        }

        public void Post(string query, string data, IJsExecutable callback)
        {
            Post(query, data, callback, null);
        }

        public async void Post(string query, string data, IJsExecutable callback, object state)
        {
            using (var req = CreateRequest())
            {
                try
                {
                    var content = new StringContent(data);
                    var r = await req.PostAsync(query, content);

                    if (callback != null)
                    {
                        string result = await r.Content.ReadAsStringAsync();
                        callback.ExecuteCallback(_scriptEngine.Visitor, state, new WebRequestArgs(result));
                    }
                }
                catch (WebException e)
                {
                    HandleException(callback, state, e);
                }
                catch (TaskCanceledException e)
                {
                    HandleException(callback, state, new WebException("", WebExceptionStatus.Timeout));
                }
            }
        }

        public void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        private HttpClient CreateRequest()
        {
            var ub = new UriBuilder(Host);

            var handler = new HttpClientHandler();
            if (!string.IsNullOrWhiteSpace(UserName))
                handler.Credentials = new NetworkCredential(UserName, Password);

            var request = new HttpClient(handler);
            request.BaseAddress = ub.Uri;

            foreach (var header in _headers)
                request.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (!string.IsNullOrEmpty(Timeout))
                request.Timeout = TimeSpan.Parse(Timeout);

            return request;
        }

        private void HandleException(IJsExecutable callback, object state, WebException e)
        {
            var error = new WebError(e);
            if (callback != null)
                callback.ExecuteCallback(_scriptEngine.Visitor, state, new WebRequestArgs(error));
            else
                LogManager.Logger.Error(error.Message, false);
        }

        class WebRequestArgs : Args<string>
        {
            public WebRequestArgs(WebError error)
                : base(error.Message)
            {
                Error = error;
                Success = false;
            }

            public WebRequestArgs(string result)
                : base(result)
            {
                Success = true;
            }

            public bool Success { get; }

            public WebError Error { get; private set; }
        }

        class WebError : Error
        {
            public WebError(WebException exception)
                : base("WebException", GetMessage(exception))
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

            private static string GetMessage(WebException e)
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
