using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;

namespace BitMobile.Configuration
{
    public class Mixins : IContainer
    {
        private List<Mixin> mixins;

        public Mixins()
        {
            mixins = new List<Mixin>();
        }

        public void AddChild(object obj)
        {
            mixins.Add((Mixin)obj);
        }

        public object[] Controls
        {
            get
            {
                return mixins.ToArray();
            }
        }


        public object GetControl(int index)
        {
            return mixins[index];
        }
    }

    public class Mixin
    {
        private String target;
        public String Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

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

        public Mixin()
        {
        }
    }
}

