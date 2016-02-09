using Jint.Expressions;

namespace ScriptEngine.BitMobile
{
    interface IJsExecutable
    {
        void ExecuteCallback(IJintVisitor visitor, object state, object args);
    }
}
