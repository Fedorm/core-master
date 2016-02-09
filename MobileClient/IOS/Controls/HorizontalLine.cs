using System;
using System.Collections.Generic;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [Synonym("line")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "HorizontalLine")]
    public class HorizontalLine : Control<UIView>
    {
        public override void CreateView()
        {
            _view = new UIView();
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color
            _view.BackgroundColor = stylesheet.Helper.BackgroundColor(this).ToColorOrClear();

            return styleBound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color
                _view.BackgroundColor = helper.Get<IBackgroundColor>().ToColorOrClear();
            }
            return styleBound;
        }

        protected override void Dismiss()
        {
            // nope
        }
    }
}