using System;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Script")]
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Script : IScript, IContainer
    {
        public Script()
        {
            WarmupActions = new WarmupActions();
            GlobalEvents = new GlobalEvents();
            Mixins = new Mixins();
            GlobalModules = new GlobalModules();
        }

        public IGlobalModules GlobalModules { get; set; }

        public IMixins Mixins { get; set; }

        public IGlobalEvents GlobalEvents { get; set; }

        public IWarmupActions WarmupActions { get; set; }

        public void AddChild(object obj)
        {
            // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
            if (obj is GlobalEvents)
                GlobalEvents = (GlobalEvents)obj;
            if (obj is GlobalModules)
                GlobalModules = (GlobalModules)obj;
            if (obj is Mixins)
                Mixins = (Mixins)obj;
            if (obj is WarmupActions)
                WarmupActions = (WarmupActions)obj;
            // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
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
