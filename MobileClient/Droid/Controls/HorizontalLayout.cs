using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "HorizontalLayout")]
    [Synonym("hl")]
    // ReSharper disable once UnusedMember.Global
    public class HorizontalLayout : CustomLayout
    {
        public HorizontalLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Horizontal(ContainerBehaviour.Childrens, styleBound, maxBound, out borders);
        }
    }
}