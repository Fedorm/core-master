using System;
using System.Text;
using Android.Content;
using System.Net;
using System.IO;
using BitMobile.Droid.Application;
using Gcm.Client;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.Utils;

namespace BitMobile.Droid.PushNotifications
{
    class Manager
    {
        private readonly AndroidApplicationContext _applicationContext;
        private readonly IGlobalEventsController _events;
        private string _host;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _deviceId;
        
        public Manager(AndroidApplicationContext applicationContext, IGlobalEventsController events, String host, String userName, String password, String deviceId)
        {
            _applicationContext = applicationContext;
            _events = events;
            _host = host;
            _userName = userName;
            _password = password;
            _deviceId = deviceId;
        }

        public void StartRegisterDevice(Context ctx)
        {
            GcmClient.CheckDevice(ctx);
            GcmClient.CheckManifest(ctx);

            if (!GcmClient.IsRegistered(ctx))
            {
                try
                {
                    string projectId;
                    try
                    {
                        _host = _host.ToCurrentScheme(false);
                        projectId = GetGoogleProjectId();
                    }
                    catch (WebException e)
                    {
                        _host = _host.ToCurrentScheme(true);
                        projectId = GetGoogleProjectId();
                    }

                    GcmBroadcastReceiver.SenderIds = new[] { projectId };
                    GcmClient.Register(ctx, GcmBroadcastReceiver.SenderIds);
                }
                catch (Exception e)
                {
                    _applicationContext.HandleException(e);
                }
            }
        }

        public void OnMessage(string message)
        {
            _events.OnPushMessage(message);
        }

        private String GetGoogleProjectId()
        {
            HttpWebRequest req = CreateRequest("/push/android/project");
            req.Method = "GET";

            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                if (resp == null)
                {
                    throw new WebException("null response");
                }
                string result;
                using (var r = new StreamReader(resp.GetResponseStream()))
                {
                    result = r.ReadToEnd();
                }
                resp.Close();

                return result;
            }
            catch (WebException e)
            {
                throw GetException(e);
            }
        }

        public void SendToken(String token)
        {
            HttpWebRequest req = CreateRequest("/push/register");
            req.Headers.Add("deviceId", _deviceId);
            req.Headers.Add("os", "android");
            req.Headers.Add("package", BitBrowserApp.Current.BaseActivity.PackageName);

            req.Headers.Add("token", token);
            req.Method = "GET";
            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                using (var r = new StreamReader(resp.GetResponseStream()))
                    r.ReadToEnd();
                resp.Close();
            }
            catch (WebException e)
            {
                _applicationContext.HandleException(GetException(e));
            }
        }

        private HttpWebRequest CreateRequest(string query)
        {
            var ub = new UriBuilder(String.Format(@"{0}/{1}", _host, query));
            var request = (HttpWebRequest)WebRequest.Create(ub.Uri);
            request.Timeout = 5000;
            string svcCredentials =
                Convert.ToBase64String(Encoding.ASCII.GetBytes(_userName + ":" + _password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            return request;
        }

        private Exception GetException(WebException e)
        {
            if (e.Response == null)
            {
                return e;
            }

            using (WebResponse response = e.Response)
            {
                using (Stream edata = response.GetResponseStream())
                using (var reader = new StreamReader(edata))
                {
                    return new WebException(reader.ReadToEnd());
                }
            }
        }
    }
}