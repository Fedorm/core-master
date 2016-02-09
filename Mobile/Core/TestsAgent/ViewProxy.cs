using BitMobile.Application;
using BitMobile.ValueStack;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace BitMobile.TestsAgent
{
    // TODO: Пока что не буду рефлекшн убирать
    public abstract class ViewProxy
    {
        protected IApplicationContext _context;

        public ViewProxy(IApplicationContext context)
        {
            _context = context;
        }

        public object Execute(string method, string[] parameters)
        {
            object result = null;

            _context.Wait();

            try
            {
                object[] preparedParameters = PrepareParameters(parameters);

                MethodInfo mi = this.GetType().GetMethod(string.Format("Do{0}", method));
                if (mi != null)
                    result = mi.Invoke(this, preparedParameters);
                else
                    result = string.Format("Error: Method {0} is not supported", method);

            }
            catch (TargetInvocationException e)
            {
                result = string.Format("Error: {0}", e.InnerException.Message);
            }
            catch (Exception e)
            {
                result = string.Format("Error: {0}", e.Message);
            }

            return result;
        }

        object[] PrepareParameters(string[] parameters)
        {
            object[] methodParams;

            if (parameters.Length > 0)
            {
                methodParams = new object[parameters.Length];

                object obj;
                if (parameters[0] != "null")
                    obj = FindInValueStack(parameters[0]);
                else
                    obj = null;

                Array.Copy(parameters, methodParams, parameters.Length);
                methodParams[0] = obj;
            }
            else
                methodParams = new object[0];
            return methodParams;
        }

        object FindInValueStack(string key)
        {
            object value = null;

            string[] strings = key.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string[] variableStrings = strings[0].Split('[', ']');

            if (_context.ValueStack.Values.TryGetValue(variableStrings[0], out value))
            {
                if (variableStrings.Length > 1)
                    value = GetValueByIndex(value, variableStrings[1]);

                for (int i = 1; i < strings.Length; i++)
                {
                    if (value == null)
                        throw new Exception(
                            string.Format("Value {0} is null. Cannot find property {1}", strings[i - 1], strings[i]));

                    string[] propStrings = strings[i].Split('[', ']');

                    PropertyInfo pi = value.GetType().GetProperty(propStrings[0]);
                    if (pi != null)
                        value = pi.GetValue(value);
                    else if (value is IIndexedProperty)
                        value = ((IIndexedProperty)value).GetValue(propStrings[0]);
                    else
                        throw new Exception(string.Format("Property {0}{1} not exist", strings[i - 1], strings[i]));

                    if (propStrings.Length > 1)
                        value = GetValueByIndex(value, propStrings[1]);
                }
            }
            else
                throw new Exception(string.Format("Cannot find {0} in Variables", strings[0]));

            return value;
        }

        object GetValueByIndex(object collection, string index)
        {
            object result = null;

            if (collection is Array)
            {
                int id;
                if (int.TryParse(index, out id))
                    result = ((Array)collection).GetValue(id);
                else
                    throw new Exception(string.Format("Index {0) has to be an integer"));
            }
            else if (collection is IDictionary)
                result = ((IDictionary)collection)[index];
            else if (collection is IIndexedProperty)
                result = ((IIndexedProperty)collection).GetValue(index);
            else
                throw new Exception(string.Format("{0} is not an indexer", collection));

            return result;
        }


        public abstract bool DoClick(object obj);

        public abstract bool DoSetFocus(object obj);

        public abstract bool DoSetText(object obj, string text);

        public abstract Stream DoTakeScreenshot();

        public virtual object DoGetValue(object obj)
        {
            return obj;
        }

        public virtual object DoGetCount(object obj)
        {
            int result = 0;

            if (obj is ICollection)
                result = ((ICollection)obj).Count;
            else if (obj is IEnumerable)
                foreach (object item in ((IEnumerable)obj))
                    result++;
            else
                throw new Exception(string.Format("{0} is not a collection", obj));

            return result;
        }

        public abstract bool DoSetValue(object obj, string property, object value);

        public abstract bool DoScrollTo(object obj, string index);

        public abstract bool DoDialogClickPositive();

        public abstract bool DoDialogClickNegative();

        public abstract string DoDialogGetMessage();

        public abstract string DoDialogGetDateTime();

        /// <param name="hack">Because we added object as first param</param>
        public abstract bool DoDialogSetDateTime(object hack, string value);

        /// <param name="hack">Because we added object as first param</param>
        public abstract bool DoDialogSelectItem(object hack, string value);

        /// <param name="hack">Because we added object as first param</param>
        public abstract string DoDialogGetItem(object hack, string value);

    }
}