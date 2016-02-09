using BitMobile.Common.ScriptEngine;

namespace BitMobile.Application.ScriptEngine
{
    public static class ScriptEngineContext
    {
        public static IScriptEngineContext Current { get; private set; }

        public static void Init(IScriptEngineContext context)
        {
            Current = context;
        }
    }
}
