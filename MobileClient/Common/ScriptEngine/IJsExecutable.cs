using System;

namespace BitMobile.Common.ScriptEngine
{
    public interface IJsExecutable
    {
        [Obsolete]
        void ExecuteStandalone(object visitor, params object[] parameters);

        void ExecuteCallback(object visitor, object state, object args);
    }
}