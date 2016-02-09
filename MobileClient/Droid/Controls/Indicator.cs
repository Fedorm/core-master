using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Indicator")]
    // ReSharper disable UnusedMember.Global, UnusedMember.Global
    class Indicator : Control<ProgressBar>
    {
        public Indicator(BaseScreen activity)
            : base(activity)
        {
            Activity = activity;
            _visible = false;
        }

        public void Start()
        {
            Activity.Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
            View.Visibility = ViewStates.Visible;
            CurrentContext.JokeProviderInternal.OnSync();
        }

        public void Stop()
        {
            Activity.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            View.Visibility = ViewStates.Invisible;
        }

        public override void CreateView()
        {
            _view = new ProgressBar(Activity, null, Android.Resource.Attribute.ProgressBarStyle);
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // color
            _view.IndeterminateDrawable.SetColorFilter(stylesheet.Helper.Color(this).ToColorOrTransparent()
                , PorterDuff.Mode.Multiply);

            return StyleSheetContext.Current.StrechBoundInProportion(styleBound, maxBound, 1, 1);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                IColor color;
                if (helper.TryGet(out color))
                    _view.IndeterminateDrawable.SetColorFilter(color.ToColorOrTransparent(), PorterDuff.Mode.Multiply);
            }

            return StyleSheetContext.Current.StrechBoundInProportion(styleBound, maxBound, 1, 1);
        }
    }
}