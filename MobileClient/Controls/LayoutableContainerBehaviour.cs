using System;
using System.Collections.Generic;
using System.Xml;
using BitMobile.Application;
using BitMobile.Application.BusinessProcess;
using BitMobile.Common.Controls;

namespace BitMobile.Controls
{
    class LayoutableContainerBehaviour<T> : ILayoutableContainerBehaviour<T>
        where T : class, ILayoutable
    {
        private readonly ILayoutableContainer _container;
        private readonly List<T> _childrens;

        public LayoutableContainerBehaviour(ILayoutableContainer container)
        {
            _container = container;
            _childrens = new List<T>();
        }

        public IList<T> Childrens { get { return _childrens.AsReadOnly(); } }

        public void Insert(int index, object obj)
        {
            var child = obj as T;
            if (child == null)
                throw new Exception(string.Format("Incorrect child: {0}. Expected type: {1}", obj, typeof(T)));
            Insert(index, child);
        }

        public void Insert(int index, T child)
        {
            if (index < 0 || index > _childrens.Count)
                throw new Exception("Index out of bounds");

            _childrens.Insert(index, child);
            child.Parent = _container;
        }

        public void Withdraw(int index)
        {
            if (index < 0 || index >= _childrens.Count)
                throw new Exception("Index out of bounds");

            T control = _childrens[index];
            _childrens.RemoveAt(index);
            control.Dispose();
        }

        public void Inject(int index, string xml)
        {
            // check for include
            if (!xml.TrimStart(' ').StartsWith("<"))
                xml = string.Format("<s:Include File=\"{0}\"/>", xml);

            string text = string.Format(
                "<Root xmlns:c=\"BitMobile.Controls\" xmlns:s=\"BitMobile.ValueStack\">{0}</Root>", xml);

            var doc = new XmlDocument();
            doc.LoadXml(text);

            XmlNode node = doc.DocumentElement.FirstChild;
            while (node != null)
            {
                var obj = BusinessProcessContext.Current.CreateObjectFactory()
                    .Build(null, node, ApplicationContext.Current.ValueStack);

                var childs = obj as object[]; // uglu hack, see details in ObjectFactory.DoInclude
                if (childs != null)
                    foreach (var child in childs)
                        _container.Insert(index++, child);
                else
                    _container.Insert(index++, obj);

                node = node.NextSibling;
            }

            _container.CreateChildrens();
        }
    }
}