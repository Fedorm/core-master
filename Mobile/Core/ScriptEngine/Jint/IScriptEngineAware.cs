using System;

namespace Jint
{
	public interface IScriptEngineAware
	{
		void SetContext(object context);
	}
}

