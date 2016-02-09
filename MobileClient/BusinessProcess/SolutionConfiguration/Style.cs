using System;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Style")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class Style : IStyle, IContainer
    {
        public Style()
        {
            DefaultStyles = new DefaultStyles();
        }

        public IDefaultStyles DefaultStyles { get; set; }

        public void AddChild(object obj)
        {
            var styles = obj as DefaultStyles;
            if (styles != null)
                DefaultStyles = styles;
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
