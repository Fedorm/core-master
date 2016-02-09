using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    [MarkupElement(MarkupElementAttribute.ValueStackNamespace, "Push")]
    public class Push : ValueStackTag, IPush
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }
    }
}