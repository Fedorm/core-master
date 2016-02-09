using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    [Synonym("dl")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "DockLayout")]
    public class DockLayout : CustomLayout
    {
        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Dock(ContainerBehaviour.Childrens, styleBound, maxBound);
        }
    }
}