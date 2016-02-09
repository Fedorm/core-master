using Android.Views;
using Android.Widget;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;

namespace BitMobile.Controls
{
    // ReSharper disable UnusedMember.Global, UnusedMember.Global
    class Indicator : Control<ProgressBar>, IImageContainer
    {
        public Indicator(BaseScreen activity)
            : base(activity)
        {
            _activity = activity;
            _visible = false;
        }

        public void Start()
        {
            _activity.Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
            View.Visibility = ViewStates.Visible;
        }

        public void Stop()
        {
            _activity.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            View.Visibility = ViewStates.Invisible;
        }

        public override View CreateView()
        {
            _view = new ProgressBar(_activity, null, Android.Resource.Attribute.ProgressBarStyle);

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // color
            _view.IndeterminateDrawable.SetColorFilter(style.ColorOrTransparent<Color>(this)
                , Android.Graphics.PorterDuff.Mode.Multiply);

            return styleBound;
        }

        #region IImageContainer

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public bool InitializeImage(StyleSheet.StyleSheet stylesheet)
        {
            // Is is not a magic numbers, just 1
            ImageWidth = 1;
            ImageHeight = 1;

            return true;
        }
        #endregion
    }
}