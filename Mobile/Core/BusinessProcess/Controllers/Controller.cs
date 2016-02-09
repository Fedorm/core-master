using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Script;
using BitMobile.ValueStack;
using BitMobile.Utilities.Develop;

namespace BitMobile.Controllers
{
    public class Controller : IExternalFunction
    {
        private Dictionary<String, Controller> globalControllers;

        private ScriptEngine scriptEngine;
        public ScriptEngine ScriptEngine
        {
            get
            {
                return scriptEngine;
            }
        }

        public Controller()
        {
        }

        public void Init(ScriptEngine scriptEngine)//, GlobalEventsController globalEventsController)
        {
            this.scriptEngine = scriptEngine;
        }

        public object CallFunction(string functionName, object[] parameters)
        {
            try
            {
                TimeStamp.Start("CallFunction: " + functionName);
                TimeCollector.Start("CallFunction");

                return scriptEngine.CallFunction(functionName, parameters);
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
                return scriptEngine.CallFunctionNoException(functionName, parameters);
            }
            finally
            {
                TimeCollector.Pause("CallFunctionNoException");
            }
        }

        public object CallVariable(string varName)
        {
            return scriptEngine.CallVariable(varName);
        }

    }
}
