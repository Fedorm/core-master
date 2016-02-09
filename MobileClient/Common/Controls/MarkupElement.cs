using System;

namespace BitMobile.Common.Controls
{
    public class MarkupElementAttribute : Attribute
    {
        public const string ValueStackNamespace = "BitMobile.ValueStack";
        public const string ControlsNamespace = "BitMobile.Controls";
        public const string BusinessProcessNamespace = "BitMobile.BusinessProcess";
        public const string ConfigurationNamespace = "BitMobile.Configuration";
        
        public MarkupElementAttribute(string nameSpace, string name)
        {
            NameSpace = nameSpace;
            Name = name;
        }

        public string NameSpace { get; private set; }

        public string Name { get; private set; }
    }
}
