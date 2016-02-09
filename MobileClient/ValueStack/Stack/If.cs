using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    [MarkupElement(MarkupElementAttribute.ValueStackNamespace, "If")]
    public class If : ValueStackTag, IIf
    {
        public string Test { get; set; }

        public virtual bool Evaluate(object value)
		{
			return (bool)value;
		}
    }

    [MarkupElement(MarkupElementAttribute.ValueStackNamespace, "Else")]
    public class Else : ValueStackTag, IElse
    {
	}
}