using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Controls;
using BitMobile.Utilities.LogManager;

namespace BitMobile.Droid.UI
{
    public abstract class Control<T> : Control where T : View
    {
        protected T _view;
        protected bool _visible = true;
        bool _disposed = false;

        public Control(BaseScreen activity)
            : base(activity)
        {
        }

        public override bool Visible
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
        public override View View
        {
            get
            {
                return _view;
            }
        }

        public override Bound Apply(StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            if (_view == null)
                throw new NullReferenceException("Cannot apply styles: View is null.");

            _view.Visibility = _visible ? ViewStates.Visible : ViewStates.Invisible;
            return styleBound;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_view != null)
                    _view.Dispose();
                _view = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}