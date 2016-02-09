using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint
{
    public interface IScriptEngineContext
    {
        object GetContext();
        ITypeResolver GetTypeResolver();
		IMethodInvoker GetMethodInvoker(ExecutionVisitor visitor);
        String ModuleName { get; }
    }
}
