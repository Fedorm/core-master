using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BitMobile.Utilities;

namespace MobileClient.Tests
{
    class PrivateObject
    {
        object _obj;
        Type _type;

        public PrivateObject(object obj, Type type)
        {
            _obj = obj;
            _type = type;
        }

        public object Invoke(string name, params object[] args)
        {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                types[i] = args[i] != null ? args[i].GetType() : typeof(object);


            BindingFlags flag = _obj != null ? BindingFlags.Instance | BindingFlags.Static : BindingFlags.Static;
            MethodInfo[] methods = _type.GetMethods(BindingFlags.NonPublic | flag);

            MethodInfo mi = null;
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m.Name == name)
                {
                    bool correct = false;
                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length == args.Length)
                    {
                        correct = true;
                        for (int j = 0; j < p.Length; j++)
                        {
                            Type t = p[j].ParameterType;
                            if (t != types[j] && !t.IsSubclassOf(types[j]))
                            {
                                correct = false;
                                break;
                            }
                        }
                    }
                    if (correct)
                        mi = m;
                }
            }


            if (mi == null)
                throw new Exception("Cannot find method '{0}' in '{1}".Format((object)name, _type));

            return mi.Invoke(_obj, args);
        }
    }
}
