using Android.App;
using Android.Content;
using Android.Support.V4.App;
using BitMobile.Application.Translator;
using BitMobile.Common.Device.Providers;
using Android.Media;

namespace BitMobile.Droid.Providers
{
    class LocalNotificationProvider : ILocalNotificationProvider
    {
        private readonly BaseScreen _activity;
        private int _msgId;

        public LocalNotificationProvider(BaseScreen activity)
        {
            _activity = activity;
        }

        public void Notify(string title, string message)
        {
            using (var intent = new Intent())
            {
                intent.SetComponent(new ComponentName(_activity, "dart.androidapp.ContactsActivity"));
                using (var pendingIntent = PendingIntent.GetActivity(_activity, 0, intent, 0))
                using (var builder = new NotificationCompat.Builder(_activity))
                {
                    var notification = builder.SetContentIntent(pendingIntent)
                        .SetContentTitle(title)
                        .SetContentText(message)
                        .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                        .SetSmallIcon(Resource.Drawable.Icon)
                        .SetAutoCancel(true)
                        .Build();

                    using (notification)
                    using (var nMgr = (NotificationManager)_activity.GetSystemService(Context.NotificationService))
                        nMgr.Notify(_msgId++, notification);
                }
            }
        }
    }
}