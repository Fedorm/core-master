using System;

using BitMobile.Script;

namespace BitMobile.ClientModel
{
	public class ContextAwareObject : Jint.IScriptEngineAware
	{
		ScriptEngine context;
		
		public void SetContext (object context)
		{
			this.context = (ScriptEngine)context;
		}

		public ScriptEngine Context {
			get {
				return context;
			}
		}
	}
}

