using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;

namespace BitMobile.Configuration
{
    public class Configuration : IContainer
    {
        public Configuration()
        {
        }
        
        private BusinessProcess businessProcess;

        public BusinessProcess BusinessProcess
        {
            get { return businessProcess; }
            set { businessProcess = value; }
        }

        private Script script = new Script();

        public Script Script
        {
            get { return script; }
            set { script = value; }
        }

        private Style style = new Style();

        public Style Style
        {
            get { return style; }
            set { style = value; }
        }

        public void AddChild(object obj)
        {
            if (obj is BusinessProcess)
                businessProcess = (BusinessProcess)obj;
            if (obj is Style)
                style = (Style)obj;
            if (obj is Script)
                script = (Script)obj;
        }

        public object[] Controls
        {
            get 
            {
                throw new NotImplementedException(); 
            }
        }

        public object GetControl(int index)
        {
            throw new NotImplementedException();
        }
    }
}
