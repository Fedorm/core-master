using System;
using System.IO;
using BitMobile.Common.Debugger;

namespace BitMobile.Common.ScriptEngine
{
    public interface IScriptEngineContext
    {
        IScriptEngine LoadScript(Stream scriptStream, string name, IDebugger debugger);
        void RegisterType(string name, Type type);
    }
}