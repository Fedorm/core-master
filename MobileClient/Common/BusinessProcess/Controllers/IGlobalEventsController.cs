using System;

namespace BitMobile.Common.BusinessProcess.Controllers
{
    public interface IGlobalEventsController
    {
        object InvokeEvent(String methodName, object[] parameters);
        void OnApplicationInit();
        void OnApplicationRestore(object workflow);
        void OnApplicationBackground(object workflow);
        void OnApplicationShake(object workflow);
        void OnPushMessage(object message);
    }
}
