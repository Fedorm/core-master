using BitMobile.BusinessProcess.Controllers;

namespace BitMobile.BusinessProcess.ClientModel
{
    class CurrentController
    {
        public void OnPushMessage(string message)
        {
            ScreenController.Current.OnPushMessage(message);
        }
    }
}
