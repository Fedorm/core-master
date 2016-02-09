using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    [Synonym("hl")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "HorizontalLayout")]
    public class HorizontalLayout : CustomLayout
    {
        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Horizontal(ContainerBehaviour.Childrens, styleBound, maxBound, out borders);
        }
    }
}