using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.ValueStack;

namespace BitMobile.Controllers
{
    public class ScreenController : Controller
    {
        public String GetView(String defaultView)
        {
            return defaultView;
        }

        public void OnLoading()
        {
            CallFunctionNoException("OnLoading", new object[] { });
        }

        public void OnLoad()
        {
            CallFunctionNoException("OnLoad", new object[] { });
        }

    }
}
