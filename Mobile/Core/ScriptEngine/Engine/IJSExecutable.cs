using System;
using Jint.Expressions;

namespace BitMobile.Script
{
    public interface IJSExecutable
    {
        [Obsolete]
        void ExecuteStandalone(IJintVisitor visitor, params object[] parameters);

        void ExecuteCallback(IJintVisitor visitor, object state, object args);
    }
}