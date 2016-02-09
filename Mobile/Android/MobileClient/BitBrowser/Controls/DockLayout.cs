using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    [Synonym("dl")]
    // ReSharper disable once UnusedMember.Global
    public class DockLayout : CustomLayout
    {
        public DockLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
           return LayoutBehaviour.Dock(stylesheet, this, Childrens, styleBound, maxBound);
        }
    }
}
