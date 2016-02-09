using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using BitMobile.Application;
using BitMobile.Application.Controls;
using BitMobile.Application.ValueStack;
using BitMobile.Common.Application;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.Controls;
using BitMobile.Common.Develop;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.Factory
{
    public class ObjectFactory : IObjectFactory
    {
        private static Dictionary<string, Dictionary<string, Type>> _types;
        private static readonly IDictionary<Type, IDictionary<string, PropertyInfo>> PropertyBag
            = new Dictionary<Type, IDictionary<string, PropertyInfo>>();
        private static readonly IDictionary<Type, Tuple<ConstructorInfo, ParameterInfo[]>> ConstructorBag
            = new Dictionary<Type, Tuple<ConstructorInfo, ParameterInfo[]>>();

        public object CreateObject(IValueStack stack, System.IO.Stream stream)
        {
            var doc = new XmlDocument();
            doc.Load(stream);
            return Build(null, doc.DocumentElement, stack);
        }

        Type FindType(string @namespace, string name)
        {
            if (_types == null)
            {
                _types = new Dictionary<string, Dictionary<string, Type>>();
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    foreach (Type type in a.GetTypes())
                    {
                        var attribute = type.GetCustomAttribute<MarkupElementAttribute>(false);
                        if (attribute != null)
                        {
                            Dictionary<string, Type> typesInNamespace;
                            if (!_types.TryGetValue(attribute.NameSpace, out typesInNamespace))
                            {
                                typesInNamespace = new Dictionary<string, Type>();
                                _types.Add(attribute.NameSpace, typesInNamespace);
                            }
                            typesInNamespace.Add(attribute.Name, type);
                        }
                    }
            }

            try
            {
                return _types[@namespace][name];
            }
            catch (KeyNotFoundException e)
            {
                throw new Exception(String.Format("Type '{0}.{1}' is not found.", @namespace, name), e);
            }
        }

        Type TypeFromNode(XmlNode node)
        {
            var t = FindType(node.NamespaceURI, node.LocalName);
            if (t == null)
                throw new Exception("Type " + String.Format("{0}.{1}", node.NamespaceURI, node.LocalName) + " is not found.");
            return t;
        }

        public object Build(IContainer parent, XmlNode node, IValueStack stack)
        {
            if (node is XmlComment)
                return null;

            Type t = TypeFromNode(node);

            if (node.NamespaceURI == MarkupElementAttribute.ValueStackNamespace)
            {
                //control flow tag
                var tag = (IValueStackTag)CreateObject(t, stack);
                foreach (XmlAttribute a in node.Attributes)
                {
                    PropertyInfo pi = FindProperty(t, a.Name);
                    //string value = ApplicationContext.Current.Dal.TranslateString(a.Value);
                    pi.SetValue(tag, a.Value);
                }

                var push = tag as IPush;
                if (push != null)
                    return DoPush(push, parent, stack);

                var iif = tag as IIf;
                if (iif != null)
                    return DoIf(iif, parent, node, stack);

                if (tag is IElse)
                    return parent;

                var iterator = tag as IIterator;
                if (iterator != null)
                    return DoIterator(iterator, parent, node, stack);

                var include = tag as IInclude;
                if (include != null)
                    return DoInclude(include, parent, stack);

                throw new Exception("Unknown tag");
            }
            //ui control

            object obj = CreateObject(t, stack);

            SetProperties(node, stack, obj);

            string id = null;
            var layoutable = obj as ILayoutable;
            if (layoutable != null)
                id = layoutable.Id;

            if (parent != null)
                parent.AddChild(obj);

            if (obj is IDataBind)
                (obj as IDataBind).DataBind();

            if (id != null)
            {
                stack.Push(id, obj); // add current object to value stack
                LoadStepState(stack, obj, id);
            }

            var container = obj as IContainer;
            if (container != null)
                ProcessChildren(container, node, stack);

            return obj;
        }

        private static object CreateObject(Type t, IValueStack stack)
        {
            Tuple<ConstructorInfo, ParameterInfo[]> ctor = FindConstructor(t);
            ConstructorInfo ci = ctor.Item1;
            ParameterInfo[] parameters = ctor.Item2;

            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                args[i] = stack.Values[parameters[i].Name];

            object obj = ci.Invoke(args);

            var aware = obj as IApplicationContextAware;
            if (aware != null)
            {
                var context = (IApplicationContext)stack.Values["context"];
                aware.SetApplicationContext(context);
            }

            return obj;
        }

        private static void LoadStepState(IValueStack stack, object obj, string id)
        {
            var persistable = obj as IPersistable;
            if (persistable != null)
            {
                stack.Persistables.Add(id, persistable);

                var context = (IApplicationContext)stack.Values["context"];

                object state;
                if (context.Workflow.BusinessProcess.AllowStatePersist
                    && context.Workflow.CurrentStep.State.TryGetValue(id, out state))
                    persistable.SetState(state);
            }
        }

        private static void SetProperties(XmlNode node, IValueStack stack, object obj)
        {
            foreach (XmlAttribute a in node.Attributes)
            {
                if (string.IsNullOrEmpty(a.Prefix))
                {
                    PropertyInfo prop = FindProperty(obj.GetType(), a.Name);
                    String aValue = a.Value;

                    if (prop.PropertyType == typeof(IDataBinder))
                    {
                        object bindedObject;
                        String propertyName;

                        stack.Evaluate(aValue, out bindedObject, out propertyName);

                        var binder = ControlsContext.Current.CreateDataBinder(obj, a.Name, bindedObject, propertyName);
                        prop.SetValue(obj, binder, null);
                    }
                    else if (prop.PropertyType == typeof(IActionHandler))
                    {
                        var handler = ControlsContext.Current.CreateActionHandler(aValue, stack);
                        prop.SetValue(obj, handler, null);
                    }
                    else if (prop.PropertyType == typeof(IActionHandlerEx))
                    {
                        var handler = ControlsContext.Current.CreateActionHandlerEx(aValue, stack, obj);
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


        private object DoInclude(IInclude tag, IContainer parent, IValueStack stack)
        {
            var dal = ApplicationContext.Current.Dal;

            object component = CreateObject(stack, dal.GetScreenByName(tag.File));
            if (!(component is IComponent))
                throw new Exception("Only Component container is allowed to be included");

            object[] controls = ((IContainer)component).Controls;
                        
            if (parent != null)
            {
                foreach (var control in controls)
                    parent.AddChild(control);
                return parent;
            }

            // uglu hack: так как метод Build возвращает object, а нам нужно вернуть несколько объектов
            // , мы можем вернуть тут массив, но должны помнить,
            // что это массив (см. Controls/LayoutableContainerBehaviour.cs метод Inject)
            return controls;
        }

        private IContainer DoIf(IIf tag, IContainer parent, XmlNode node, IValueStack stack)
        {
            bool value = stack.BooleanExpression(tag.Test);

            if (value)
                ProcessChildren(parent, node, stack);
            else
            {
                XmlNode ifNode = node.NextSibling;
                if (ifNode != null && !(ifNode is XmlComment))
                {
                    Type t = TypeFromNode(ifNode);
                    if (t.GetInterfaces().Contains(typeof(IElse)))
                        ProcessChildren(parent, ifNode, stack);
                }
            }

            return parent;
        }

        private IContainer DoIterator(IIterator tag, IContainer parent, XmlNode node, IValueStack stack)
        {
            var collection = (IEnumerable)stack.Evaluate(tag.Value, null, false);

            IIteratorStatus status = ValueStackContext.Current.CreateIteratorStatus();
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

        private IContainer DoPush(IPush tag, IContainer parent, IValueStack stack)
        {
            Type type = null;
            if (!string.IsNullOrEmpty(tag.Type))
                type = GetType().Assembly.GetType(tag.Type, false, true);

            object expr = stack.Evaluate(tag.Value, type);
            stack.Push(tag.Id, expr);
            return parent;
        }

        private void ProcessChildren(IContainer parent, XmlNode node, IValueStack stack)
        {
            XmlNode child = node.FirstChild;
            while (child != null)
            {
                Build(parent, child, stack);
                child = child.NextSibling;
            }
        }

        private static PropertyInfo FindProperty(Type type, string name)
        {
            IDictionary<string, PropertyInfo> bag;
            if (!PropertyBag.TryGetValue(type, out bag))
            {
                bag = new Dictionary<string, PropertyInfo>();
                PropertyBag.Add(type, bag);
            }

            PropertyInfo pi;
            if (!bag.TryGetValue(name, out pi))
            {
                pi = type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (pi == null)
                    throw new Exception(String.Format("Property '{0}' is not found in type '{1}'", name, type.Name));
                bag.Add(name, pi);
            }

            return pi;
        }

        private static Tuple<ConstructorInfo, ParameterInfo[]> FindConstructor(Type type)
        {
            Tuple<ConstructorInfo, ParameterInfo[]> constructor;
            if (!ConstructorBag.TryGetValue(type, out constructor))
            {
                ConstructorInfo[] constructors = type.GetConstructors();
                if (constructors.Length == 0)
                    throw new Exception(String.Format("Unable to create instance of {0}", type.FullName));

                ConstructorInfo ci = constructors[constructors.Length - 1]; //last one..
                ParameterInfo[] parameters = ci.GetParameters();
                constructor = new Tuple<ConstructorInfo, ParameterInfo[]>(ci, parameters);

                ConstructorBag.Add(type, constructor);
            }

            return constructor;
        }
    }
}