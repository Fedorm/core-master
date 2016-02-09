using System;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Views;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "HorizontalLine")]
    [Synonym("line")]
    // ReSharper disable once UnusedMember.Global
    class HorizontalLine : Control<View>
    {
        public HorizontalLine(BaseScreen activity)
            : base(activity)
        {
        }

        public override void CreateView()
        {
            _view = new View(Activity);
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color
            _view.SetBackgroundColor(stylesheet.Helper.BackgroundColor(this).ToColorOrTransparent());

            return styleBound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, background image, borders
                IBackgroundColor backgroundColor;
                if (helper.TryGet(out backgroundColor))
                    _view.SetBackgroundColor(backgroundColor.ToColorOrTransparent());
            }

            return styleBound;
        }
    }
}