using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;

namespace BitMobile.Configuration
{
    public class GlobalModules : IContainer
    {
        private List<Module> modules;

        public GlobalModules()
        {
            modules = new List<Module>();
        }

        public void AddChild(object obj)
        {
            modules.Add((Module)obj);
        }

        public object[] Controls
        {
            get 
            { 
                return modules.ToArray(); 
            }
        }


        public object GetControl(int index)
        {
            return modules[index];
        }
    }
        
    public class Module
    {
        private String name;
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
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

        public Module()
        {
        }
    }
}

