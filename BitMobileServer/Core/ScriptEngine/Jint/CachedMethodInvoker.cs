using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Jint.Native;

namespace Jint
{
    class CachedMethodInvoker : IMethodInvoker
    {
        private readonly ExecutionVisitor _visitor;

        readonly Dictionary<string, Dictionary<string, MethodInfo>> _cache = new Dictionary<string, Dictionary<string, MethodInfo>>();

        public CachedMethodInvoker(ExecutionVisitor visitor)
        {
            _visitor = visitor;
        }

        public string GetCacheKey(string method, object[] parameters, Type[] generics)
        {
            var sb = new StringBuilder();
            sb.Append(method).Append(';');
            foreach (var o in parameters)
            {
                if (o != null)
                {
                    sb.Append(o.GetType().FullName).Append(';');
                }
                else
                {
                    sb.Append("null").Append(';');
                }
            }
            if (generics.Length > 0)
            {
                sb.Append("<");
                foreach (var o in generics)
                {
                    sb.Append(o.FullName);
                }
                sb.Append("]");
            }

            return sb.ToString();
        }

        public MethodInfo Invoke(object subject, string method, ref object[] parameters, Type[] generics)
        {
            MethodInfo methodInfo = null;
            var key = GetCacheKey(method, parameters, generics);

            // Static evaluation
			var isStaticCall = subject is Type;
            var type = isStaticCall ? (Type)subject : subject.GetType();

            if (!_cache.ContainsKey(type.FullName))
            {
                _cache.Add(type.FullName, new Dictionary<string, MethodInfo>());
            }

            if (!_cache[type.FullName].ContainsKey(key))
            {
                var ms = new List<MethodInfo>();

                foreach (var m in isStaticCall ? type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) : type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    // check name of method and parameters number
                    if (m.Name != method || m.GetParameters().Length != parameters.Length) continue;
                    
                    // is the method is generic ? 
                    ms.Add(m.GetGenericArguments().Length > 0 ? m.MakeGenericMethod(generics) : m);
                }

                #region Search exact parameter types
                foreach (var m in ms)
                {
                    var pis = m.GetParameters();

                    var compatible = true;
                    // check types compatibility
                    for (var i = 0; i < pis.Length; i++)
                    {
                        // polymorphic 
                        if (parameters[i] != null)
                        {
                            if (pis[i].ParameterType == parameters[i].GetType())
                            {
                                continue;
                            }
                        }
                        else if (!pis[i].ParameterType.IsValueType)
                        {
                            continue;
                        }
                        
                        compatible = false;
                        break;
                    }

                    if (!compatible)
                    {
                        continue;
                    }

                    methodInfo = m;
                }
                #endregion


                if (methodInfo == null)
                {
                    #region Search compatible parameter types
                    foreach (var m in ms)
                    {
                        var pis = m.GetParameters();

                        var compatible = true;
                        // check types compatibility
                        for (var i = 0; i < pis.Length; i++)
                        {
                            // plymorphic 
                            if (parameters[i] != null)
                            {
                                if (pis[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
                                {
                                    continue;
                                }
                            }
                            else if (pis[i].ParameterType.IsByRef)
                            {
                                continue;
                            }

                            compatible = false;
                            break;
                        }

                        if (!compatible)
                        {
                            continue;
                        }

                        methodInfo = m;
                        break;
                    }
                    #endregion
                }

                if (methodInfo == null)
                {
                    #region Search matching parameter types
                    foreach (var m in ms)
                    {
                        var pis = m.GetParameters();

                        var compatible = TryGetAppropriateParameters(parameters, pis, subject);

                        if (!compatible)
                        {
                            continue;
                        }

                        methodInfo = m;
                        break;
                    }
                    #endregion
                }

                if (methodInfo != null)
                {
                    _cache[type.FullName].Add(key, methodInfo);
                }
            }
            else
            {
                methodInfo = _cache[type.FullName][key];
                var pis = methodInfo.GetParameters();

                GetAppropriateParameters(parameters, pis, subject);

            }

            return methodInfo;
        }

        public void GetAppropriateParameters(object[] parameters, Type[] pis, object subject)
        {
            if (!TryGetAppropriateParameters(parameters, pis, subject))
                throw new JintException("Could not get appropriate parameters");
        }

        public void GetAppropriateParameters(object[] parameters, ParameterInfo[] pis, object subject)
        {
            if (pis == null)
                return;
            GetAppropriateParameters(parameters, ConvertParameterInfos(pis), subject);
        }

        public static Type[] ConvertParameterInfos(ParameterInfo[] pis)
        {
            var ps = new Type[pis.Length];
            for (int i = 0; i < pis.Length; i++)
            {
                ps[i] = pis[i].ParameterType;
            }

            return ps;
        }

        public bool TryGetAppropriateParameters(object[] parameters, ParameterInfo[] pis, object subject)
        {
            return pis == null || TryGetAppropriateParameters(parameters, ConvertParameterInfos(pis), subject);
        }

        public bool TryGetAppropriateParameters(object[] parameters, Type[] pis, object subject)
        {
            var compatible = true;
            for (var i = 0; i < pis.Length; i++)
            {
                // plymorphic 
                if (parameters[i] != null)
                {
                    if (pis[i].IsInstanceOfType(parameters[i]))
                    {
                        continue;
                    }
                }
                else if (!pis[i].IsValueType)
                {
                    continue;
                }

                try
                {
                    if (pis[i].IsArray)
                    {
                        // try to convert every elements
                        var array = parameters[i] as JsArray;

                        if (array == null)
                        {
                            compatible = false;
                            break;
                        }

                        var newArray = Array.CreateInstance(pis[i], array.Length);

                        for (var k = 0; k < array.Length; k++)
                        {
                            newArray.SetValue(array[k.ToString()], k);
                        }
                    }
					/*
                    else if (typeof(Delegate).IsAssignableFrom(pis[i])) // wrap the JsFunction to a Delegate
                    {
                        var dw = new DelegateWrapper
                                     {
                                         Visitor = _visitor,
                                         Function = (JsFunction) parameters[i],
                                         That = _visitor.Global.WrapClr(subject)
                                     };

                        var dm = DelegateWrapper.GenerateDynamicMethod(pis[i]); // void (int)

                        parameters[i] = dm.CreateDelegate(pis[i], dw);
                    }
                    */
                    else if (pis[i].IsEnum && parameters[i] is string)
                    {
                        var enumNames = Enum.GetNames(pis[i]);
                        foreach (string name in ((string)parameters[i]).Split(' '))
                        {
                            if (Array.IndexOf(enumNames, name) >= 0) continue;

                            compatible = false;
                            break;
                        }
                        if (!compatible)
                            break;
                        parameters[i] = Enum.Parse(pis[i], (string)parameters[i]);
                    }
                    else
                    {
                        parameters[i] = Convert.ChangeType(parameters[i], pis[i]);
                    }
                }
                catch
                {
                    compatible = false;
                    break;
                }
            }
            return compatible;
        }
    }
}
