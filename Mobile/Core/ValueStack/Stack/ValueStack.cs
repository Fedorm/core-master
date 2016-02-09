using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.LogManager;
using BitMobile.ExpressionEvaluator;
using Common.Controls;

namespace BitMobile.ValueStack
{
    public class ValueStack : IEvaluator
    {
        public const string VALIDATE_ALL = "all";

        public CustomExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }
        readonly CustomExceptionHandler _exceptionHandler;

        private Dictionary<String, object> values = new CustomDictionary();
        public Dictionary<String, object> Values
        {
            get
            {
                return values;
            }
        }

        public Dictionary<string, IPersistable> Persistables { get; private set; }

        public ValueStack(CustomExceptionHandler handler)
        {
            _exceptionHandler = handler;
            Persistables = new Dictionary<string, IPersistable>();
        }

        public void Push(String name, object value)
        {
            String fullName = name;
            String[] parts = name.Split('.');
            Dictionary<String, object> dict = values;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                String part = parts[i];
                if (dict.ContainsKey(part))
                {
                    if (dict[part] is Dictionary<string, object>)
                    {
                        dict = (Dictionary<string, object>)dict[part];
                        continue;
                    }
                }
                throw new Exception("Invalid variable expression: " + fullName);
            }

            if (value != null)
                if (value.Equals("null"))
                    value = null;

            name = parts[parts.Length - 1];
            if (dict.ContainsKey(name))
                dict.Remove(name);
            dict.Add(name, value);
        }

        public object Pull(String name)
        {
            object result = null;

            if (values.TryGetValue(name, out result))
                values.Remove(name);

            return result;
        }

        public object Peek(string name)
        {
            object result = null;

            if (values.ContainsKey(name))
                result = values[name];

            return result;
        }

        public bool BooleanExpression(String expression, object root = null)
        {
            if (expression.StartsWith("$") && expression.Contains("("))
                return (bool)CallScript(expression);
            if (expression.StartsWith("@"))
                return (bool)CallVariable(expression);

            ExpressionFactory factory = new ExpressionFactory(this.Values);

            Func<object, bool> func = factory.BuildLogicalExpression(expression);

            return func(null);
        }

        private IEnumerable<String> Subexpressions(String expression)
        {
            String[] arr = expression.Split('{');
            foreach (String s in arr)
            {
                if (s.Contains("}"))
                    yield return s.Substring(0, s.IndexOf("}"));
            }
        }

        public object Evaluate(String expression, object root)
        {
            object obj = root;
            String[] parts = expression.Split('.');
            for (int i = 1; i < parts.Length; i++)
            {
                String part = parts[i];

                if (obj is BitMobile.DbEngine.IDbRecordset)
                {
                    /*
                    int idx = ((System.Data.IDataRecord)obj).GetOrdinal(part);
                    if (idx == -1)
                        throw new Exception(String.Format("Recordset does not contain field '{0}'", part));
                    obj = ((System.Data.IDataRecord)obj).GetValue(idx);
                    */
                    obj = ((BitMobile.DbEngine.IDbRecordset)obj).GetValue(part);
                    if (obj is DbEngine.DbRef)
                        continue;
                    break;
                }

                if (obj is DbEngine.DbRef)
                    obj = ((DbEngine.DbRef)obj).GetObject();

                if (obj == null)
                    return null;

                if (obj is IIndexedProperty)
                {
                    IIndexedProperty ip = (IIndexedProperty)obj;
                    if (!ip.HasProperty(part))
                        return null;
                    else
                        obj = ip.GetValue(part);
                }
                else
                if (obj is Dictionary<String, object>)
                {
                    Dictionary<String, object> dict = (Dictionary<String, object>)obj;
                    if (!dict.ContainsKey(part))
                        return null;
                    obj = dict[part];
                }
                else
                {
                    PropertyInfo pi = obj.GetType().GetProperty(part);
                    if (pi != null)
                        obj = pi.GetValue(obj, null);
                    else
                    {
                        MethodInfo mi = typeof(ExpressionHelper).GetMethod(parts[i], BindingFlags.Static | BindingFlags.Public);
                        if (mi != null)
                            obj = mi.Invoke(null, new object[] { obj });
                        else
                        {
                            mi = obj.GetType().GetMethod(parts[i], new Type[] { Values["dao"].GetType() });
                            if (mi != null)
                                obj = mi.Invoke(obj, new object[] { Values["dao"] });
                            else
                                throw new Exception(String.Format("Invalid expression: {0}", expression));
                        }
                    }
                }
                if (obj == null)
                    return null;
            }
            return obj;
        }

        public void PrepareScriptCall(String expression, out String module, out String func, out object[] parameters)
        {
            int pos1 = expression.IndexOf("(");
            int pos2 = expression.IndexOf(")");
            module = "";
            func = expression.Substring(1, pos1 - 1);
            String[] arr = func.Split('.');
            if (arr.Length > 2)
                throw new Exception(String.Format("Invalid expression '{0}'", expression));
            if (arr.Length == 2)
            {
                module = arr[0];
                func = arr[1];
            }

            String[] args = expression.Substring(pos1 + 1, pos2 - pos1 - 1).Split(',');
            parameters = new object[args.Length];
            int i = 0;
            foreach (String arg in args)
            {
                parameters[i] = Evaluate(arg);
                i++;
            }
        }

        object CallVariable(String expression)
        {
            String varName = expression.Substring(1);
            IExternalFunction func = Values["controller"] as IExternalFunction;
            return func.CallVariable(varName);
        }

        object CallScript(String expression)
        {
            String module;
            String func;
            object[] parameters;

            PrepareScriptCall(expression, out module, out func, out parameters);

            return CallScript(func, parameters);
        }

        public object CallScript(String functionName, object[] parameters)
        {
            IExternalFunction func = Values["controller"] as IExternalFunction;
            return func.CallFunction(functionName, parameters);
        }

        public object TryCallScript(String functionName, params object[] parameters)
        {
            object controller;
            if (Values.TryGetValue("controller", out controller))
                return (controller as IExternalFunction).CallFunctionNoException(functionName, parameters);
            else
                return null;
        }

        public object Evaluate(String expression, Type type = null, bool canString = true)
        {
            expression = expression.Trim();

            if (expression.StartsWith("$") && expression.Contains("("))
                return CallScript(expression);
            if (expression.StartsWith("@"))
                return CallVariable(expression);

            foreach (String se in Subexpressions(expression))
            {
                object obj = Evaluate(se);
                expression = expression.Replace("{" + se + "}", obj != null ? obj.ToString() : "");
            }
            expression = expression.Replace("\"", "");

            if (expression.StartsWith("$"))
            {
                String[] parts = expression.Split('.');
                object obj = null;

                if (!parts[0].StartsWith("$"))
                    throw new Exception("Invalid expression: " + expression);
                parts[0] = parts[0].Remove(0, 1);

                if (parts.Length < 1)
                    throw new Exception(String.Format("Invalid expression: ${0}", expression));

                if (!Values.ContainsKey(parts[0]))
                    //throw new Exception(String.Format("Unable to find variable: ${0}", parts[0]));
                    return null;

                obj = Values[parts[0]];
                if (obj == null)
                    return null;

                obj = Evaluate(expression.Remove(0, 1), obj);

                if (obj == null)
                    return null;

                if (type != null)
                {
                    if (!obj.GetType().Equals(type) && !type.IsAssignableFrom(obj.GetType()))
                        obj = Convert.ChangeType(obj, type);
                }
                return obj;
            }
            else
            {
                if (!canString)
                    throw new Exception(String.Format("Expression '{0}' have to be a function", expression));

                if (expression.ToLower().Equals("null"))
                    return null;
                if (type == null)
                    type = typeof(String);
                return System.Convert.ChangeType(expression, type);
            }
        }

        public void Evaluate(String expression, out object obj, out String propertyName)
        {
            expression = expression.Trim();

            if (expression.StartsWith("$"))
            {
                String[] parts = expression.Split('.');

                if (parts.Length < 2)
                    throw new Exception(String.Format("Invalid expression: {0}", expression));

                propertyName = parts[parts.Length - 1];
                obj = Evaluate(expression.Substring(0, expression.Length - propertyName.Length - 1));
            }
            else
            {
                throw new Exception("Evaluate expression error - constant is not allowed: " + expression);
            }
        }

        private Func<object> GetExpressionFunc2(String expr)
        {
            Parser p = new Parser();
            IEnumerable<String> keys = values.Keys.OrderBy(x => x);
            foreach (String key in keys)
            {
                expr = expr.Replace("$" + key, key);
                p.RegisterType(key, values[key]);
            }

            return p.CompileExpression(expr, this.values);
        }

        public string GetContentString()
        {
            string result = "<ValueStack>";

            foreach (KeyValuePair<string, object> pair in values)
                if (pair.Key != "context" && pair.Key != "dao" && pair.Key != "activity")
                {
                    result += Environment.NewLine;
                    result += " <" + pair.Key + ">";
                    result += Environment.NewLine;
                    result += LogSerializer.ObjToString(pair.Value, 1, 0);
                    result += " </" + pair.Key + ">";
                }

            result += Environment.NewLine;
            result += "</ValueStack>";
            return result;
        }
    }
}