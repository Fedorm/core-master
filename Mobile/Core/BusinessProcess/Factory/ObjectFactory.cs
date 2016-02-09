using System;
using System.Collections.Generic;
using System.Data;
using BitMobile.Application;
using BitMobile.ValueStack;
using BitMobile.Controls;
using System.Collections;
using System.Reflection;
using System.Xml;
using Common.Controls;

namespace BitMobile.Factory
{
    public class ObjectFactory
    {
        static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public object CreateObject(ValueStack.ValueStack stack, System.IO.Stream stream)
        {
            var doc = new XmlDocument();
            doc.Load(stream);
            return Build(null, doc.DocumentElement, stack);
        }

        Type FindType(String typeName)
        {
            Type result;

            if (!_types.TryGetValue(typeName, out result))
            {
                result = GetType().Assembly.GetType(typeName, false, true);
                if (result == null)
                {
                    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        result = a.GetType(typeName, false, true);
                        if (result != null)
                            break;
                    }
                }
                if (result == null)
                    throw new Exception(String.Format("Type '{0}' is not found.", typeName));

                _types.Add(typeName, result);
            }
            return result;
        }

        Type TypeFromNode(XmlNode node)
        {
            var t = FindType(String.Format("{0}.{1}", node.NamespaceURI, node.LocalName));
            if (t == null)
                throw new Exception("Type " + String.Format("{0}.{1}", node.NamespaceURI, node.LocalName) + " is not found.");
            return t;
        }

        object Build(IContainer parent, XmlNode node, ValueStack.ValueStack stack)
        {
            if (node is XmlComment)
                return null;

            Type t = TypeFromNode(node);

            if (t.IsSubclassOf(typeof(ValueStackTag)))
            {
                //control flow tag
                ConstructorInfo ci = t.GetConstructor(new Type[] { });
                var tag = (ValueStackTag)ci.Invoke(new object[] { });
                foreach (XmlAttribute a in node.Attributes)
                {
                    PropertyInfo pi = t.GetProperty(a.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					string value = ApplicationContext.Context.DAL.TranslateString (a.Value);
					pi.SetValue(tag, Convert.ChangeType(value, pi.PropertyType), null);
                }

                if (tag is Include)
                {
                    return DoInclude((Include)tag, parent, stack);
                }

                if (tag is If)
                {
                    return DoIf((If)tag, parent, node, stack);
                }

                if (tag is Else)
                {
                    return parent;
                }

                if (tag is Iterator)
                {
                    return DoIterator((Iterator)tag, parent, node, stack);
                }

                if (tag is Push)
                {
                    object newObject = null;
                    if (node.ChildNodes.Count == 1)
                    {
                        newObject = Build(null, node.FirstChild, stack);
                    }
                    return DoPush((Push)tag, parent, stack, newObject);
                }

                throw new Exception("Unknown tag");
            }
            else
            {
                //ui control

                object obj = CreateObject(t, stack);

                string id = InitObjectId(node, stack, obj);

                SetProperties(node, stack, obj);

                if (parent != null)
                    parent.AddChild(obj);

                if (obj is IDataBind)
                    (obj as IDataBind).DataBind();

                if (id != null)
                    LoadStepState(stack, obj, id);

                if (obj is IContainer)
                    ProcessChildren((IContainer)obj, node, stack);

                return obj;
            }
        }

        private static object CreateObject(Type t, ValueStack.ValueStack stack)
        {
            object obj;
            MethodInfo mi = t.GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public);
            if (mi != null)
            {
                ParameterInfo[] parameters = mi.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = stack.Values[parameters[i].Name];
                }
                obj = mi.Invoke(null, args);
            }
            else
            {

                ConstructorInfo[] constructors = t.GetConstructors();
                if (constructors.Length == 0)
                    throw new Exception(String.Format("Unable to create instance of {0}", t.FullName));
                ConstructorInfo ci = constructors[constructors.Length - 1]; //last one..
                ParameterInfo[] parameters = ci.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    args[i] = stack.Values[parameters[i].Name];
                obj = ci.Invoke(args);
            }

            var context = (IApplicationContext)stack.Values["context"];

            var aware = obj as IApplicationContextAware;
            if (aware != null)
                aware.SetApplicationContext(context);

            return obj;
        }

        private static string InitObjectId(XmlNode node, ValueStack.ValueStack stack, object obj)
        {
            foreach (XmlAttribute a in node.Attributes)
            {
                if (string.IsNullOrEmpty(a.Prefix) && a.Name.ToLower().Equals("id"))
                {
					string aValue = ApplicationContext.Context.DAL.TranslateString (a.Value);
                    string id = stack.Evaluate(aValue).ToString();
                    stack.Push(id, obj);
                    return id;
                }
            }
            return null;
        }

        private static void LoadStepState(ValueStack.ValueStack stack, object obj, string id)
        {
            var persistable = obj as IPersistable;
            if (persistable != null)
            {
                stack.Persistables.Add(id, persistable);

                var context = (IApplicationContext)stack.Values["context"];

                object state;
                if (context.Workflow.CurrentStep.State.TryGetValue(id, out state))
                    persistable.SetState(state);
            }
        }

        private static void SetProperties(XmlNode node, ValueStack.ValueStack stack, object obj)
        {
            foreach (XmlAttribute a in node.Attributes)
            {
                if (String.IsNullOrEmpty(a.Prefix))
                {
                    PropertyInfo prop = obj.GetType()
                        .GetProperty(a.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        throw new Exception(String.Format("Property '{0}' is not found in type '{1}'", a.Name,
                            obj.GetType().Name));

					String aValue = ApplicationContext.Context.DAL.TranslateString(a.Value.Trim());

                    if (prop.PropertyType == typeof(DataBinder))
                    {
                        object bindedObject;
                        String propertyName;
                        stack.Evaluate(aValue, out bindedObject, out propertyName);

                        var binder = new DataBinder(obj, a.Name, bindedObject, propertyName);
                        prop.SetValue(obj, binder, null);
                    }
                    else if (prop.PropertyType == typeof(ActionHandler))
                    {
                        var handler = new ActionHandler(aValue, stack);
                        prop.SetValue(obj, handler, null);
                    }
                    else if (prop.PropertyType == typeof(ActionHandlerEx))
                    {
                        var handler = new ActionHandlerEx(aValue, stack, obj);
                        prop.SetValue(obj, handler, null);
                    }
                    else
                    {
                        object mexpr = stack.Evaluate(aValue, prop.PropertyType);
                        prop.SetValue(obj, mexpr, null);
                    }
                }
            }
        }


        private IContainer DoInclude(Include tag, IContainer parent, ValueStack.ValueStack stack)
        {
            var dal = ApplicationContext.Context.DAL;

            object component = CreateObject(stack, dal.GetScreenByName(tag.File));
            if (!(component is Component))
                throw new Exception("Only Component container is allowed to be included");

            object[] controls = ((IContainer)component).Controls;
            foreach (object child in controls)
                parent.AddChild(child);
            return parent;
        }

        private IContainer DoIf(If tag, IContainer parent, XmlNode node, ValueStack.ValueStack stack)
        {
            bool value = stack.BooleanExpression(tag.Test);

            if (value)
                ProcessChildren(parent, node, stack);
            else
            {
                XmlNode ifNode = node.NextSibling;
                if (ifNode != null)
                {
                    Type t = TypeFromNode(ifNode);
                    if (t == typeof(Else))
                        ProcessChildren(parent, ifNode, stack);
                }
            }

            return parent;
        }

        private IContainer DoIterator(Iterator tag, IContainer parent, XmlNode node, ValueStack.ValueStack stack)
        {
            var collection = (IEnumerable)stack.Evaluate(tag.Value, null, false);

            var status = new IteratorStatus();
            if (!String.IsNullOrEmpty(tag.Status))
                stack.Push(tag.Status, status); //push status variable

            var list = new ArrayList();
            var reader = collection as IDataReader;
            if (reader != null)
            {
                while (reader.Read())
                {
                    stack.Push(tag.Id, reader);
                    ProcessChildren(parent, node, stack);
                    status.Inc();
                }
            }
            else
            {
                foreach (var item in collection)
                    list.Add(item);

                foreach (var item in list)
                {
                    stack.Push(tag.Id, item);
                    ProcessChildren(parent, node, stack);
                    status.Inc();
                }
            }

            if (!String.IsNullOrEmpty(tag.Status))
                stack.Values.Remove(tag.Status); //remove status variable

            return parent;
        }

        private IContainer DoPush(Push tag, IContainer parent, ValueStack.ValueStack stack, object newObject = null)
        {
            object expr = newObject;
            if (expr == null)
            {
                Type type = null;
                if (!String.IsNullOrEmpty(tag.Type))
                    type = GetType().Assembly.GetType(tag.Type, false, true);
                /*
                if (!String.IsNullOrEmpty(tag.Value))
                    if (!tag.Value.StartsWith("$")) //TODO
                        throw new ArgumentException(String.Format("Invalid Push expression '{0}'. Should start with '$'", tag.Value));
                */
                expr = stack.Evaluate(tag.Value, type);
            }

            stack.Push(tag.Id, expr);
            return parent;
        }

        private void ProcessChildren(IContainer parent, XmlNode node, ValueStack.ValueStack stack)
        {
            XmlNode child = node.FirstChild;
            while (child != null)
            {
                Build(parent, child, stack);
                child = child.NextSibling;
            }
        }
    }
}