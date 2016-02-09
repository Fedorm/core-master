using System;
using System.Linq;
using Android.App;
using Android.Content;
using BitMobile.Application.Exceptions;
using Gcm.Client;

namespace BitMobile.Droid.PushNotifications
{
    [Service] //Must use the service tag
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GcmService : GcmServiceBase
    {
        public GcmService()
            : base(GcmBroadcastReceiver.SenderIds)
        { }

        protected override void OnRegistered(Context context, string registrationId)
        {
            //Receive registration Id for sending GCM Push Notifications to
            PushNotificationsManagerFactory.GetInstance().SendToken(registrationId);
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            //Receive notice that the app no longer wants notifications
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            //Push Notification arrived - print out the keys/values
            if (intent != null && intent.Extras != null)
            {
                var data = intent.Extras.KeySet().ToDictionary(key => key, key => intent.Extras.Get(key).ToString());
                
                string alert;
                data.TryGetValue("alert", out alert);

                if (!string.IsNullOrEmpty(alert))
                    PushNotificationsManagerFactory.GetInstance().OnMessage(alert);
            }
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            //Some recoverable error happened
            BitBrowserApp.Current.AppContext.HandleException(new NonFatalException("GcmService recoverable error! Error id: " + errorId));
            return false;
        }

        protected override void OnError(Context context, string errorId)
        {
            //Some more serious error happened
            BitBrowserApp.Current.AppContext.HandleException(new Exception("GcmService error! Error id: " + errorId));
        }

    }
}