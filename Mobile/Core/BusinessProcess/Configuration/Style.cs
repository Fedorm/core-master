using BitMobile.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Configuration
{
    public class Style : IContainer
    {
        private DefaultStyles styles = new DefaultStyles();

        public DefaultStyles DefaultStyles
        {
            get { return styles; }
            set { styles = value; }
        }

        public void AddChild(object obj)
        {
            if (obj is DefaultStyles)
                styles = (DefaultStyles)obj;
        }

        public object[] Controls
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public Style()
        {
        }


        public object GetControl(int index)
        {
            throw new NotImplementedException();
        }
    }
}
