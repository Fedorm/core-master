using System;
using BitMobile.Common.Device.Providers;
using MonoTouch.UIKit;

namespace BitMobile.IOS.Providers
{
    class LocalNotificationProvider: ILocalNotificationProvider
    {
        public void Notify(string title, string message)
        {
            var notification = new UILocalNotification
            {
                FireDate = DateTime.Now,
                AlertAction = title,
                AlertBody = message,
                HasAction = true,
                SoundName = UILocalNotification.DefaultSoundName
            };
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }
    }
}
