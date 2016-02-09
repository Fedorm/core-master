using System;
using BitMobile.Common.Debugger;

namespace BitMobile.Common.ScriptEngine
{
    public interface IScriptEngine
    {
        object Visitor { get; }
        IDebugger Debugger { get; }
        object CallFunction(string name, params object[] args);
        object CallFunctionNoException(string name, params object[] args);
        object CallVariable(string varName);
        void AddVariable(string name, object value);
        void ApplyBreakPoints();
        Exception CreateException(object innerObject);
    }
}
