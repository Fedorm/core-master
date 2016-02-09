using System;
using BitMobile.Common.Debugger;
using BitMobile.Common.ScriptEngine;
using BitMobile.Common.ValueStack;
using Jint;
using Jint.Debugger;
using Jint.Native;

namespace BitMobile.Script
{
    public class ScriptEngine : JintEngine, Jint.IScriptEngineContext, IExternalFunction, IScriptEngine
    {
        private IDebugger debugger;
        public IDebugger Debugger
        {
            get
            {
                return debugger;
            }
        }

        public ScriptEngine(String moduleName, IDebugger debugger, Options options = Options.Ecmascript5 | Options.Strict)
            : base(options)
        {
            this.debugger = debugger;
            this.moduleName = moduleName;
            SetDebugMode(debugger != null);
        }


        public object Visitor
        {
            get
            {
                return visitor;
            }
        }

        public void AddVariable(String name, object value)
        {
            this.SetParameter(name, value);
        }

        public void ApplyBreakPoints()
        {
            Break -= OnBreakEvent;
            int[] breakPoints = Debugger.GetBreakPoints(ModuleName);
            if (breakPoints != null)
            {
                if (breakPoints.Length > 0)
                {
                    BreakPoints.Clear();
                    foreach (int line in breakPoints)
                    {
                        var bp = new BreakPoint(line, 0);
                        BreakPoints.Add(bp);
                    }
                    Break += OnBreakEvent;
                }
            }
        }

        public Exception CreateException(object innerObject)
        {
            var jsClr = new JsClr(visitor, innerObject);
            return new JsException(jsClr);
        }

        public object GetContext()
        {
            return this;
        }

        public ITypeResolver GetTypeResolver()
        {
            return new CachedTypeResolver();
        }

        public IMethodInvoker GetMethodInvoker(ExecutionVisitor visitor)
        {
            return new MethodInvoker(this);
        }

        public override void WriteToDebugConsole(string name, params object[] args)
        {
            if (debugger != null)
            {

                String s = "";
                foreach (object arg in args)
                {
                    if (s != "")
                        s = s + ", ";
                    s = s + (arg == null ? "null" : arg.ToString());
                }
                debugger.WriteToConsole(String.Format("{0}::{1}({2})", moduleName, name, s));
            }
        }

        private void OnBreakEvent(object sender, DebugInformation e)
        {
            Debugger.OnBreak(sender, e);
        }

    }
}