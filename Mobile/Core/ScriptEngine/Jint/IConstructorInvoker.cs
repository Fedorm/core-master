using System;
using System.Reflection;

namespace Jint
{
    public interface IConstructorInvoker
    {
        ConstructorInfo Invoke(Type type, object[] parameters);
    }
}
