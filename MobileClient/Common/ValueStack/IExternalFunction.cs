using System;

namespace BitMobile.Common.ValueStack
{
    public interface IExternalFunction
    {
        object CallFunction(String functionName, object[] parameters);
        object CallFunctionNoException(String functionName, object[] parameters);
        object CallVariable(String varName);
    }
}

