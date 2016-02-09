using System;
using BitMobile.Common.Controls;
using System.Collections.Generic;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Component")]
    // ReSharper disable UnusedMember.Global
    public class Component : IComponent
    {
        private readonly List<object> _list = new List<object>();

        public void AddChild(object obj)
        {
            _list.Add(obj);                
        }

        // ReSharper disable once UnusedParameter.Global
        public void Insert(int index, object obj)
        {
            AddChild(obj);
        }

        public object[] Controls
        {
            get { return _list.ToArray(); }
        }

        public object GetControl(int index)
        {
            return _list[index];
        }
    }
}
