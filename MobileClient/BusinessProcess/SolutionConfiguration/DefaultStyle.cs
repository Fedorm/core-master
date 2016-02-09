using System.Collections.Generic;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "DefaultStyles")]
    public class DefaultStyles : IDefaultStyles, IContainer
    {
        private readonly List<DefaultStyle> _styles;

        public DefaultStyles()
        {
            _styles = new List<DefaultStyle>();
        }

        public void AddChild(object obj)
        {
            _styles.Add((DefaultStyle)obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _styles.ToArray();
            }
        }

        public object GetControl(int index)
        {
            return _styles[index];
        }
    }

    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "DefaultStyle")]
    public class DefaultStyle
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string File { get; set; }
    }
}

