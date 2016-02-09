using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;

namespace BitMobile.Configuration
{
    public class WarmupActions : IContainer
    {
        private List<WarmupAction> actions;

        public WarmupActions()
        {
            actions = new List<WarmupAction>();
        }

        public void AddChild(object obj)
        {
            actions.Add((WarmupAction)obj);
        }

        public object[] Controls
        {
            get 
            {
                return actions.ToArray(); 
            }
        }
        
        public object GetControl(int index)
        {
            return actions[index];
        }
    }
        
    public class WarmupAction
    {
        private String controller;
        public String Controller
        {
            get
            {
                return controller;
            }
            set
            {
                controller = value;
            }
        }

        private String function;
        public String Function
        {
            get
            {
                return function;
            }
            set
            {
                function = value;
            }
        }

        public WarmupAction()
        {
        }
    }
}

