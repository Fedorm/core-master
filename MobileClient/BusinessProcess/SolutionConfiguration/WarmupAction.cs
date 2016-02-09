using System.Collections.Generic;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "WarmupActions")]
    public class WarmupActions : IWarmupActions, IContainer
    {
        private readonly List<WarmupAction> _actions;

        public WarmupActions()
        {
            _actions = new List<WarmupAction>();
        }

        public void AddChild(object obj)
        {
            _actions.Add((WarmupAction)obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _actions.ToArray();
            }
        }

        public object GetControl(int index)
        {
            return _actions[index];
        }
    }

    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "WarmupAction")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WarmupAction
    {
        public string Controller { get; set; }

        public string Function { get; set; }
    }
}

