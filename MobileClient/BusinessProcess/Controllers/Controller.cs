using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.Develop;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.Controllers
{
    public class Controller : IController
    {
        public IScriptEngine ScriptEngine { get; private set; }

        public void Init(IScriptEngine scriptEngine)
        {
            ScriptEngine = scriptEngine;
        }

        public object CallFunction(string functionName, object[] parameters)
        {
            try
            {
                TimeStamp.Start("CallFunction: " + functionName);
                TimeCollector.Start("CallFunction");

                return ScriptEngine.CallFunction(functionName, parameters);
            }
            finally
            {
                TimeCollector.Pause("CallFunction");
                TimeStamp.Log("CallFunction: " + functionName);
            }
        }

        public object CallFunctionNoException(string functionName, object[] parameters)
        {
            try
            {
                TimeCollector.Start("CallFunctionNoException");
                return ScriptEngine.CallFunctionNoException(functionName, parameters);
            }
            finally
            {
                TimeCollector.Pause("CallFunctionNoException");
            }
        }

        public object CallVariable(string varName)
        {
            return ScriptEngine.CallVariable(varName);
        }

    }
}
