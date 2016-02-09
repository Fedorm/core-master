using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "DockLayout")]
    [Synonym("dl")]
    // ReSharper disable once UnusedMember.Global
    public class DockLayout : CustomLayout
    {
        public DockLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this).Dock(ContainerBehaviour.Childrens, styleBound, maxBound);
        }
    }
}
