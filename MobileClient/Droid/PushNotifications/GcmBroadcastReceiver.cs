using Android.App;
using Android.Content;
using Gcm.Client;

namespace BitMobile.Droid.PushNotifications
{
    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new[] { Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new[] { "@PACKAGE_NAME@" })]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GcmBroadcastReceiver : GcmBroadcastReceiverBase<GcmService>
    {
        //IMPORTANT: Change this to your own Sender ID!
        //The SENDER_ID is your Google API Console App Project Number
        public static string[] SenderIds = { "293034226758" };
    }
}