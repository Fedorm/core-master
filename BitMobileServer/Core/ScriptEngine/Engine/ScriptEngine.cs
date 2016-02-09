using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jint;

namespace BitMobile.Script
{
    public class ScriptEngine : JintEngine, IScriptEngineContext
    {
		public event ModuleResolver ModuleResolver;

		public ScriptEngine()
		{
			this.SetDebugMode(true);
		}

		public static void RegisterType(String name, Type type)
		{
            if (!CachedTypeResolver.TypeRegistered(name))
                CachedTypeResolver.RegisterType(name, type);
		}

		public void AddVariable(String name, object value)
		{
			this.SetParameter(name, value);
		}

		public object CallFunctionNoException(String functionName)
		{
			return this.CallFunctionChecked(functionName);
		}

		private static Dictionary<String,ScriptEngine> scripts = new Dictionary<string, ScriptEngine>();
        private static Dictionary<String, DateTime> scriptsTime = new Dictionary<string, DateTime>();

		public static ScriptEngine LoadScript(System.IO.Stream scriptStream, String name, DateTime lastWriteTime)
		{
            if (scripts.ContainsKey(name))
            {
                if (scriptsTime[name] < lastWriteTime)
                {
                    scripts.Remove(name);
                    scriptsTime.Remove(name);
                }
            }

			if(scripts.ContainsKey(name))
				return scripts[name];
			else
			{
				ScriptEngine engine = new ScriptEngine();
				scripts.Add(name,engine);
                scriptsTime.Add(name, lastWriteTime);

				if(scriptStream!=null)
					engine.Run(new System.IO.StreamReader(scriptStream));

				return engine;
			}
		}

		#region IScriptEngineContext implementation
		
		public object GetContext ()
		{
			return this;
		}
		
		public ITypeResolver GetTypeResolver ()
		{
			return new CachedTypeResolver();
		}

		#region IScriptEngineContext implementation

		public IMethodInvoker GetMethodInvoker(ExecutionVisitor visitor)
		{
			return new MethodInvoker(this);
		}

		#endregion
		
		#endregion

		#region IExternalFunction implementation

		public object CallFunction(string moduleName, string functionName, object[] parameters)
		{
			ScriptEngine engine = this;
			if(!String.IsNullOrEmpty(moduleName))
			{
				if(!moduleName.EndsWith(".js"))
					moduleName = moduleName + ".js";
				if(!scripts.ContainsKey(moduleName))
				{
					if(ModuleResolver!=null)
						engine = ModuleResolver(moduleName);
					if(engine==null)
						throw new Exception(String.Format("Invalid module name '{0}'", moduleName));
				}
				else
					engine = scripts[moduleName];
			}

			try
			{
				return engine.CallFunction(functionName,parameters);
			}
			catch(Exception e)
			{
				throw new Exception(String.Format("{0}:{1}",e.Message,this.CurrentLine));
			}
		}

		#endregion
    }
}