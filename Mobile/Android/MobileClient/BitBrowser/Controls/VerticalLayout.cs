using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    [Synonym("vl")]
    // ReSharper disable once UnusedMember.Global
    public class VerticalLayout : CustomLayout
    {
        public VerticalLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            return LayoutBehaviour.Vertical(stylesheet, this, Childrens, styleBound, maxBound);
        }
    }
}