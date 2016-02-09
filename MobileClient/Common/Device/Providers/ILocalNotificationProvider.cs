namespace BitMobile.Common.Device.Providers
{
    public interface ILocalNotificationProvider
    {
        void Notify(string title, string message);
    }
}