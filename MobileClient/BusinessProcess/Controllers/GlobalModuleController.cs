using System;
using System.Reflection;
using BitMobile.ValueStack;

namespace BitMobile.BusinessProcess.Controllers
{
    // ReSharper disable UnusedMember.Global
    public class GlobalModuleController : Controller, IGlobalModule
    {
        private string _methodName;

        public MethodInfo GetProxyMethod(string methodName, object[] arguments)
        {
            _methodName = methodName;
            int cnt = arguments == null ? 0 : arguments.Length;

            if (cnt > 10)
                throw new ArgumentException("More than 10 arguments is not allowed.");

            return GetType().GetMethod(string.Format("Call{0}", cnt));
        }
        
        public object Call0()
        {
            return CallFunction(_methodName, new object[] { });
        }

        public object Call1(object p1)
        {
            return CallFunction(_methodName, new[] { p1 });
        }

        public object Call2(object p1, object p2)
        {
            return CallFunction(_methodName, new[] { p1, p2 });
        }

        public object Call3(object p1, object p2, object p3)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3 });
        }

        public object Call4(object p1, object p2, object p3, object p4)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4 });
        }

        public object Call5(object p1, object p2, object p3, object p4, object p5)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5 });
        }

        public object Call6(object p1, object p2, object p3, object p4, object p5, object p6)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5, p6 });
        }

        public object Call7(object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5, p6, p7 });
        }

        public object Call8(object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5, p6, p7, p8 });
        }

        public object Call9(object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
        }

        public object Call10(object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9, object p10)
        {
            return CallFunction(_methodName, new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });
        }
    }
}
