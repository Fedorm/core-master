using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace BitMobile.ValueStack
{
    public class Translator
    {
        private String rootFolder;

        public Translator(String rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public void Translate(ValueStack stack, System.IO.Stream input, System.IO.Stream output)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(input);

            using (System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(output, new XmlWriterSettings() { NewLineHandling = NewLineHandling.Entitize, Indent = false, NewLineOnAttributes = false }))
            {
                ProcessNode(stack, doc.DocumentElement.FirstChild, w);
                w.Flush();
            }
        }

        public void Translate(ValueStack stack, System.IO.Stream input, System.Xml.XmlWriter w)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(input);
            ProcessNode(stack, doc.DocumentElement.FirstChild, w);
        }

        private void ProcessNode(ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:

                    Type t = GetType(node);
                    if (t != null)
                    {
                        //control flow tag
                        System.Reflection.ConstructorInfo ci = t.GetConstructor(new System.Type[] { });
                        ValueStackTag tag = (ValueStackTag)ci.Invoke(new object[] { });
                        foreach (System.Xml.XmlAttribute a in node.Attributes)
                        {
                            System.Reflection.PropertyInfo pi = t.GetProperty(a.Name, BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                            pi.SetValue(tag, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
                        }

                        if (tag is Include)
                        {
                            DoInclude((Include)tag, stack, node, w);
                        }

                        if (tag is Property)
                        {
                            DoProperty((Property)tag, stack, node, w);
                        }

                        if (tag is If)
                        {
                            DoIf((If)tag, stack, node, w);
                        }

                        if (tag is Else)
                        {
                        }

                        if (tag is Iterator)
                        {
                            DoIterator((Iterator)tag, stack, node, w);
                        }
                    }
                    else
                    {
                        w.WriteStartElement(node.LocalName);
                        foreach (System.Xml.XmlAttribute a in node.Attributes)
                        {
                            if (String.IsNullOrEmpty(a.Prefix))
                            {
                                String aValue = a.Value.Trim();
                                if (String.IsNullOrEmpty(node.NamespaceURI))
                                    w.WriteAttributeString(a.LocalName, aValue);
                            }
                        }

                        ProcessChildren(stack, node, w);

                        w.WriteEndElement();
                    }
                    break;

                case XmlNodeType.Text:
                    w.WriteString(node.Value);
                    break;

                default:
                    throw new Exception("Unsupported node type");
            }

        }

        private void DoIf(If tag, ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            bool value = stack.BooleanExpression(tag.Test);
            if (value)
            {
                ProcessChildren(stack, node, w);
            }
            else
            {
                System.Xml.XmlNode ifNode = node.NextSibling;
                if (ifNode != null)
                {
                    Type t = GetType(ifNode);
                    if (t == typeof(BitMobile.ValueStack.Else))
                        ProcessChildren(stack, ifNode, w);
                }
            }
        }

        private void DoProperty(Property tag, ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            String s = tag.Value;
            if (s.StartsWith("$"))
            {
                object value = stack.Evaluate(s);
                if (value == null)
                    s = "";
                else
                    s = value.ToString();
            }
            w.WriteString(s);
        }

        private void DoInclude(Include tag, ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            String fName = String.Format(@"{0}\{1}", rootFolder, tag.File);
            using (System.IO.Stream input = System.IO.File.OpenRead(fName))
            {
                new Translator(rootFolder).Translate(stack, input, w);
            }
        }

        private void DoIterator(Iterator tag, ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            System.Collections.IEnumerable collection = (System.Collections.IEnumerable)stack.Evaluate(tag.Value, null, false);

            if (collection is System.Data.IDataReader)
            {
                while (((System.Data.IDataReader)collection).Read())
                {
                    stack.Push(tag.Id, collection as System.Data.IDataRecord);
                    ProcessChildren(stack, node, w);
                }
            }
            else
            {
                System.Collections.IEnumerator enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    stack.Push(tag.Id, enumerator.Current);
                    ProcessChildren(stack, node, w);
                }
            }
        }

        private Type FindType(String typeName)
        {
            Type result = this.GetType().Assembly.GetType(typeName);
            if (result == null)
            {
                foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    result = a.GetType(typeName, false, true);
                    if (result != null)
                        break;
                }
            }
            if (result == null)
                throw new Exception(String.Format("Type '{0}' is not found.", typeName));
            return result;
        }

        private Type GetType(System.Xml.XmlNode node)
        {
            if (!String.IsNullOrEmpty(node.NamespaceURI))
            {
                System.Type t = FindType(String.Format("{0}.{1}", node.NamespaceURI, node.LocalName));
                if (t == null)
                    throw new Exception("Type " + String.Format("{0}.{1}", node.NamespaceURI, node.LocalName) + " is not found.");
                return t;
            }
            else
                return null;
        }

        private void ProcessChildren(ValueStack stack, System.Xml.XmlNode node, System.Xml.XmlWriter w)
        {
            System.Xml.XmlNode child = node.FirstChild;
            while (child != null)
            {
                ProcessNode(stack, child, w);
                child = child.NextSibling;
            }
        }


    }
}
