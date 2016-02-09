using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    [MarkupElement(MarkupElementAttribute.ValueStackNamespace, "Iterator")]
    public class Iterator : ValueStackTag, IIterator
    {
        public string Id { get; set; }

        public string Value { get; set; }

        public string Status { get; set; }
    }

    public class IteratorStatus : IIteratorStatus
    {
        public int Index { get; set; }

        public IteratorStatus()
        {
            Index = 0;
        }

        public void Inc()
        {
            Index++;
        }
    }
}