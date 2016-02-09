using Android.Views;
using BitMobile.Common.Controls;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "TextView")]
    // ReSharper disable UnusedMember.Global
    public class TextView : CustomText<Android.Widget.TextView>
    {
        public TextView(BaseScreen activity)
            : base(activity)
        {
        }

        public override void CreateView()
        {
            _view = new Android.Widget.TextView(Activity);
            _view.SetIncludeFontPadding(false);
        }

        public override void AnimateTouch(MotionEvent e)
        {
            DoTouchAnimation(e);
        }
    }
}
