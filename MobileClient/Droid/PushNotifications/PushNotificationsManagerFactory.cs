using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Droid.Application;

namespace BitMobile.Droid.PushNotifications
{
    static class PushNotificationsManagerFactory
    {
        private static Manager _manager;

        public static Manager CreateInstance(AndroidApplicationContext context, IGlobalEventsController events
            , string host, string userName, string password, string deviceId)
        {
            return _manager ?? (_manager = new Manager(context, events, host, userName, password, deviceId));
        }

        public static Manager GetInstance()
        {
            return _manager;
        }
    }
}