using System;
using System.IO;
using System.Net;
using System.Text;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.Application;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    class PushNotificationManager
    {        
        const string SettingsTokenName = "PushDeviceToken";
		private IOSApplicationContext _applicaitonContext;
        private string _token;
        private bool _tokenNotSaved;

		public void SetApplicationContext(IOSApplicationContext applicaitonContext)
		{
			_applicaitonContext = applicaitonContext;
		}

        public void InitLocalNotifications(NSDictionary options)
        {
            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            if (options != null)
            {
                // check for a local notification
                if (options.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
                {
                    var localNotification = options[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
                    if (localNotification != null)
                        new UIAlertView(localNotification.AlertAction, localNotification.AlertBody, null, "OK", null).Show();
                }
            }
        }

        public void InitRemoteNotifications()
        {
            if (_applicaitonContext.Settings.PushDisabled)
                return;
            
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.None, new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                const UIRemoteNotificationType types = UIRemoteNotificationType.Alert
                                                       | UIRemoteNotificationType.Badge
                                                       | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(types);
            }
        }

        public void SaveCurrentToken(NSData inputDeviceToken)
        {
            _token = inputDeviceToken.Description;
            if (!string.IsNullOrWhiteSpace(_token))
                _token = _token.Trim('<', '>').Replace(" ", "");

            var oldToken = NSUserDefaults.StandardUserDefaults.StringForKey(SettingsTokenName);

            if (string.IsNullOrEmpty(oldToken) || !oldToken.Equals(_token))
                _tokenNotSaved = true;

            NSUserDefaults.StandardUserDefaults.SetString(_token, SettingsTokenName);
        }

        public void SendToken()
        {
            if (_tokenNotSaved)
            {
                HttpWebRequest req = CreateRequest("/push/register");
                req.Headers.Add("deviceId", IOSApplicationContext.UniqueID);
                req.Headers.Add("os", "ios");
                req.Headers.Add("package", NSBundle.MainBundle.BundleIdentifier);

                req.Headers.Add("token", _token);
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
                    _applicaitonContext.HandleException(GetException(e));
                    NSUserDefaults.StandardUserDefaults.SetString(string.Empty, SettingsTokenName);
                }
            }
        }

        private HttpWebRequest CreateRequest(string query)
        {
            IApplicationSettings settings = _applicaitonContext.Settings;
            var uri = new Uri(string.Format("{0}/{1}", settings.BaseUrl, query));
            var request = new HttpWebRequest(uri);
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(
                _applicaitonContext.CommonData.UserId + ":" + settings.Password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            return request;
        }

        private Exception GetException(WebException e)
        {
            using (WebResponse response = e.Response)
            {
                using (Stream edata = response.GetResponseStream())
                using (var reader = new StreamReader(edata))
                {
                    return new Exception(reader.ReadToEnd());
                }
            }
        }

        public void ReceivedRemoteNotification(NSDictionary userInfo)
        {
            string message = null;
            NSObject aps = userInfo.ObjectForKey(new NSString("aps"));
            if (aps != null && aps.IsKindOfClass(new Class(typeof(NSDictionary))))
            {
                var dict = (NSDictionary)aps;
                NSObject alert = dict.ObjectForKey(new NSString("alert"));
                if (alert != null && alert.IsKindOfClass(new Class(typeof(NSString))))
                    message = alert.ToString();
            }

            if (message != null)
                BusinessProcessContext.Current.GlobalEventsController.OnPushMessage(message);
            else
                _applicaitonContext.HandleException(new NonFatalException(D.PUSH_NOTIFICATION_ERROR, userInfo.ToString()));
        }

        public void ReceivedLocalNotification(UILocalNotification notification)
        {
            new UIAlertView(notification.AlertAction, notification.AlertBody, null, "OK", null).Show();
        }
    }
}