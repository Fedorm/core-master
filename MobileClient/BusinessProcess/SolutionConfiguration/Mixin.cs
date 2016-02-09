using System.Collections.Generic;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Mixins")]
    public class Mixins : IMixins, IContainer
    {
        private readonly List<Mixin> _mixins;

        public Mixins()
        {
            _mixins = new List<Mixin>();
        }

        public void AddChild(object obj)
        {
            _mixins.Add((Mixin)obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _mixins.ToArray();
            }
        }


        public object GetControl(int index)
        {
            return _mixins[index];
        }
    }

    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Mixin")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Mixin
    {
        public string Target { get; set; }
        
        public string File { get; set; }
    }
}

