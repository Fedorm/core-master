using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "VerticalLayout")]
    [Synonym("vl")]
    // ReSharper disable once UnusedMember.Global
    public class VerticalLayout : CustomLayout
    {
        public VerticalLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Vertical(ContainerBehaviour.Childrens, styleBound, maxBound, out borders);
        }
    }
}