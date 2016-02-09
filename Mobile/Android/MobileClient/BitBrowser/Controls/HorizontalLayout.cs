using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    [Synonym("hl")]
    // ReSharper disable once UnusedMember.Global
    public class HorizontalLayout : CustomLayout
    {
        public HorizontalLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            return LayoutBehaviour.Horizontal(stylesheet, this, Childrens, styleBound, maxBound);
        }
    }
}