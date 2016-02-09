using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Controls
{
    public class Component : IContainer
    {
        private List<object> list = new List<object>();

        public void AddChild(object obj)
        {
            list.Add(obj);
        }

        public object[] Controls
        {
            get 
            {
                return list.ToArray(); 
            }
        }

        public object GetControl(int index)
        {
            return list[index];
        }
    }
}
