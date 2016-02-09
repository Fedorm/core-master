using BitMobile.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Configuration
{
    public class Script : IContainer
    {
        private GlobalModules globalModules = new GlobalModules();

        public GlobalModules GlobalModules
        {
            get { return globalModules; }
            set { globalModules = value; }
        }

        private Mixins mixins = new Mixins();

        public Mixins Mixins
        {
            get { return mixins; }
            set { mixins = value; }
        }

        private GlobalEvents globalEvents = new GlobalEvents();

        public GlobalEvents GlobalEvents
        {
            get { return globalEvents; }
            set { globalEvents = value; }
        }

        private WarmupActions warmupActions = new WarmupActions();

        public WarmupActions WarmupActions
        {
            get { return warmupActions; }
            set { warmupActions = value; }
        }

        public void AddChild(object obj)
        {
            if (obj is GlobalEvents)
                globalEvents = (GlobalEvents)obj;
            if (obj is GlobalModules)
                globalModules = (GlobalModules)obj;
            if (obj is Mixins)
                mixins = (Mixins)obj;
            if (obj is WarmupActions)
                warmupActions = (WarmupActions)obj;
        }

        public object[] Controls
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public Script()
        {
        }


        public object GetControl(int index)
        {
            throw new NotImplementedException();
        }
    }
}
