using Android.Views;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;
using BitMobile.Utilities.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BitMobile.Droid.UI
{
    public abstract class Control : IControl<View>
    {
        protected BaseScreen _activity;
        bool _disposed = false;

        public Control(BaseScreen activity)
        {
            _activity = activity;
        }
        
        public string Id { get; set; }

        public abstract bool Visible { get; set; }

        public virtual void AnimateTouch(MotionEvent e)
        {
        }

        #region IControl<View>
        
        public abstract View CreateView();

        [NonLog]
        public abstract View View { get; }

        [NonLog]
        public object Parent { get; set; }
        #endregion

        #region ILayoutable

        public Rectangle Frame { get; set; }
        #endregion

        #region IStyledObject

        public string CssClass { get; set; }

        public abstract Bound Apply(StyleSheet stylesheet, Bound styleBound, Bound maxBound);
        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _activity = null;
                }

                _disposed = true;
            }
        }

        ~Control()
        {
            Dispose(false);
        }
    }

}