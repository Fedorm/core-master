using System;

namespace BitMobile.Common.BusinessProcess.Controllers
{
    public interface IScreenController: IController
    {
        String GetView(String defaultView);
        void OnLoading(string screenName);
        void OnLoad(string screenName);
        void SetCurrentScreenController();
    }
}
