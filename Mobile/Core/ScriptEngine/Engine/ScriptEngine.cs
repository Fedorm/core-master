using System;
using System.Collections.Generic;
using Jint;
using BitMobile.ValueStack;
using Jint.Expressions;

namespace BitMobile.Script
{
    public class ScriptEngine : JintEngine, IScriptEngineContext, IExternalFunction
    {
        static Dictionary<String, ScriptEngine> scripts = new Dictionary<string, ScriptEngine>();
        
        private BitMobile.Debugger.IDebugger debugger;
        public BitMobile.Debugger.IDebugger Debugger
        {
            get
            {
                return debugger;
            }
        }

        public static ScriptEngine LoadScript(System.IO.Stream scriptStream, String name, BitMobile.Debugger.IDebugger debugger)
        {
            if (scripts.ContainsKey(name))
                return scripts[name];
            else
            {
                ScriptEngine engine = new ScriptEngine(name, debugger);
                scripts.Add(name, engine);

                if (scriptStream != null)
                    engine.Run(new System.IO.StreamReader(scriptStream));

                return engine;
            }
        }

        public ScriptEngine(String moduleName, BitMobile.Debugger.IDebugger debugger)
        {
            this.debugger = debugger;
            this.moduleName = moduleName;
            this.SetDebugMode(debugger != null);
        }

        public IJintVisitor Visitor 
        { 
            get 
            { 
                return visitor; 
            } 
        }

        public static void RegisterType(String name, Type type)
        {
            CachedTypeResolver.RegisterType(name, type);
        }

        public void AddVariable(String name, object value)
        {
            this.SetParameter(name, value);
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

    }
}