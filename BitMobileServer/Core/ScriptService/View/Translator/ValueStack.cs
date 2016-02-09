using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

using BitMobile.Script;

namespace BitMobile.ValueStack
{
    public class ValueStack : IIndexedProperty
    {
        private Dictionary<String, object> values = new CustomDictionary();
        public Dictionary<String, object> Values
        {
            get
            {
                return values;
            }
        }

        public object this[string index]
        {
            get
            {
                return this.values[index];
            }
            set
            {
                this.values[index] = value;
            }
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

        public bool BooleanExpression(String expression, object root = null)
        {
            return (bool)Evaluate(expression);
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

        private object Evaluate(String expression, object root)
        {
            object obj = root;
            String[] parts = expression.Split('.');
            for (int i = 1; i < parts.Length; i++)
            {
                String part = parts[i];

                if (obj is ScriptService.IDbRecordset)
                {
                    obj = ((ScriptService.IDbRecordset)obj).GetValue(part);
                    break;
                }

                if (obj is Dictionary<String, object>)
                {
                    Dictionary<String, object> dict = (Dictionary<String, object>)obj;
                    if (!dict.ContainsKey(part))
                        return null;
                    obj = dict[part];
                }
                else
                {
                    System.Reflection.PropertyInfo pi = obj.GetType().GetProperty(part);
                    if (pi != null)
                        obj = pi.GetValue(obj, null);
                    else
                    {
                         System.Reflection.MethodInfo mi = obj.GetType().GetMethod(parts[i], new Type[] { Values["dao"].GetType() });
                            if (mi != null)
                                obj = mi.Invoke(obj, new object[] { Values["dao"] });
                            else
                                throw new Exception(String.Format("Invalid expression: {0}", expression));
                    }
                }
                if (obj == null)
                    return null;
            }
            return obj;
        }

        public object Evaluate(String expression, Type type = null, bool canString = true)
        {
            expression = expression.Trim();

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
                    throw new Exception(String.Format("Expression '{0}' has to be a function", expression));

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


        object CallScript(String expression)
        {
            /*
            String module;
            String func;
            object[] parameters;

            PrepareScriptCall(expression, out module, out func, out parameters);

            return scriptEngine.CallFunction(this, module, func, parameters);
            */
            throw new NotImplementedException();
        }


        public object GetValue(string propertyName)
        {
            return (values as IIndexedProperty).GetValue(propertyName);
        }

        public bool HasProperty(string propertyName)
        {
            return (values as IIndexedProperty).HasProperty(propertyName);
        }
    }
}