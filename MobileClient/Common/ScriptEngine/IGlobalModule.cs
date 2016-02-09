using System;
using System.Reflection;

namespace BitMobile.ValueStack
{
    public interface IGlobalModule
    {
        MethodInfo GetProxyMethod(String methodName, object[] arguments);
    }
}
