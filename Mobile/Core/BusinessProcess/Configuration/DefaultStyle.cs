using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;

namespace BitMobile.Configuration
{
    public class DefaultStyles : IContainer
    {
        private List<DefaultStyle> styles;

        public DefaultStyles()
        {
            styles = new List<DefaultStyle>();
        }

        public void AddChild(object obj)
        {
            styles.Add((DefaultStyle)obj);
        }

        public object[] Controls
        {
            get 
            {
                return styles.ToArray(); 
            }
        }

        public object GetControl(int index)
        {
            return styles[index];
        }
    }
        
    public class DefaultStyle
    {
        private String file;
        public String File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        public DefaultStyle()
        {
        }
    }
}

