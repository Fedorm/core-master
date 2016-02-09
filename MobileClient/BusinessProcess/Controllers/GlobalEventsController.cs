using System;
using BitMobile.Common.BusinessProcess.Controllers;

namespace BitMobile.BusinessProcess.Controllers
{
    public class GlobalEventsController : Controller, IGlobalEventsController
    {        
        public object InvokeEvent(String methodName, object[] parameters)
        {
            return CallFunctionNoException(methodName, parameters);
        }

        public void OnApplicationInit()
        {
            CallFunctionNoException("OnApplicationInit", new object[0]);
        }

        public void OnApplicationRestore(object arg1)
        {
            CallFunctionNoException("OnApplicationRestore", new[] { arg1 });
        }

        public void OnApplicationBackground(object arg1)
        {
            CallFunctionNoException("OnApplicationBackground", new[] { arg1 });
        }

        public void OnApplicationShake(object arg1)
        {
            CallFunctionNoException("OnApplicationShake", new[] { arg1 });
        }

        public void OnPushMessage(object message)
        {
            CallFunctionNoException("OnPushMessage", new[] { message });
        }
    }
}
