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
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Indicator")]
    public class Indicator : Control<UIActivityIndicatorView>
    {        
        private int _requests;

        public Indicator()
        {
            _visible = false;
        }

        public void Start()
        {
            if (_requests == 0)
            {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                _view.StartAnimating();
                CurrentContext.JokeProviderInternal.OnSync();
            }
            _requests++;
        }

        public void Stop()
        {
            if (_requests == 1)
            {
                _view.StopAnimating();
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
            }
            _requests--;
        }


        public override void CreateView()
        {
            _view = new UIActivityIndicatorView();
            _view.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
            _view.StopAnimating();
            _view.HidesWhenStopped = true;
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // color
            _view.Color = stylesheet.Helper.Color(this).ToColorOrClear();

            _view.HidesWhenStopped = true;

            return StyleSheetContext.Current.StrechBoundInProportion(styleBound, maxBound, 1, 1);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                IColor color;
                if (helper.TryGet(out color))
                    _view.Color = color.ToColorOrClear();
            }
            return styleBound;
        }

        protected override void Dismiss()
        {
            // nope
        }        
    }
}