using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    [Synonym("vl")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "VerticalLayout")]
    public class VerticalLayout : CustomLayout
    {
        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Vertical(ContainerBehaviour.Childrens, styleBound, maxBound, out borders);
        }
    }
}