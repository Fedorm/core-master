using BitMobile.Common.Application;

namespace BitMobile.BusinessProcess.ClientModel
{
    class LocalNotification
    {
        private readonly IApplicationContext _context;

        public LocalNotification(IApplicationContext context)
        {
            _context = context;
        }

        // ReSharper disable once UnusedMember.Global
        public void Notify(string title, string message)
        {
            _context.LocalNotificationProvider.Notify(title, message);
        }
    }
}