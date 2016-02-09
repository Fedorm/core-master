using System;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Log;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Droid.UI
{
    public abstract class Control<T> : Control where T : View
    {
        // ReSharper disable once InconsistentNaming
        protected T _view;
        // ReSharper disable once InconsistentNaming
        protected bool _visible = true;

        protected Control(BaseScreen activity)
            : base(activity)
        {
        }

        public sealed override bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                if (_view != null)
                    _view.Visibility = _visible ? ViewStates.Visible : ViewStates.Invisible;
            }
        }

        [NonLog]
        public sealed override View View
        {
            get
            {
                return _view;
            }
        }

        public sealed override void DismissView()
        {
            if (_view != null)
            {
                Dismiss();
                _view.Dispose();
                _view = null;
            }
        }

        protected virtual void Dismiss()
        {
            if (_view.Background != null)
            {
                _view.Background.Dispose();
                SetBackground(null);
            }
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            var bound = base.Apply(stylesheet, styleBound, maxBound);

            if (_view == null)
                throw new NullReferenceException("Cannot apply styles: View is null.");

            _view.Visibility = _visible ? ViewStates.Visible : ViewStates.Invisible;
            return bound;
        }

        protected IBound GetBoundByBackgroud(IBound styleBound, IBound maxBound)
        {
            if (_view == null)
                throw new NullReferenceException("View is null");

            var bg = _view.Background as BitmapDrawable;
            if (bg != null)
                return StyleSheetContext.Current.StrechBoundInProportion(styleBound, maxBound, bg.Bitmap.Width, bg.Bitmap.Height);

            return styleBound;
        }

        internal sealed override void SetBackground(Drawable drawable)
        {
            if (_view == null)
                throw new NullReferenceException("_view is null");

            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
                _view.SetBackgroundDrawable(drawable);
            else
                _view.Background = drawable;
        }
    }
}