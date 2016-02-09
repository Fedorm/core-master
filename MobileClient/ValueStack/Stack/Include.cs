using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    [MarkupElement(MarkupElementAttribute.ValueStackNamespace, "Include")]
    public class Include : ValueStackTag, IInclude
    {
        public string File { get; set; }
    }
}