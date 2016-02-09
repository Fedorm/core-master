using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.ValueStack;

namespace BitMobile.Controllers
{
    public class GlobalEventsController : Controller
    {
        public object InvokeEvent(String methodName, object[] parameters)
        {
            return this.CallFunctionNoException(methodName, parameters);
        }

        public void OnApplicationInit()
        {
            this.CallFunctionNoException("OnApplicationInit", new object[0]);
        }

        public void OnApplicationRestore(object arg1)
        {
            this.CallFunctionNoException("OnApplicationRestore", new object[] { arg1 });
        }

        public void OnApplicationBackground(object arg1)
        {
            this.CallFunctionNoException("OnApplicationBackground", new object[] { arg1 });
        }

        public void OnApplicationShake(object arg1)
        {
            this.CallFunctionNoException("OnApplicationShake", new object[] { arg1 });
        }

    }
}
