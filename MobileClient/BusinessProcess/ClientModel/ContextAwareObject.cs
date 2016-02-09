using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class ContextAwareObject : IScriptEngineAware
    {
        public void SetContext(object context)
        {
            Context = (IScriptEngine)context;
        }

        public IScriptEngine Context { get; private set; }
    }
}

