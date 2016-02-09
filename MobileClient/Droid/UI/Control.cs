using Android.Graphics.Drawables;
using Android.Views;
using BitMobile.Application;
using BitMobile.Common.Controls;
using BitMobile.Droid.Application;

namespace BitMobile.Droid.UI
{
    // ReSharper disable UnusedMember.Global
    public abstract class Control : CustomControl<View>, IApplicationContextAware
    {
        protected BaseScreen Activity;

        protected Control(BaseScreen activity)
        {
            Activity = activity;
        }
        
        public abstract bool Visible { get; set; }

        public sealed override IRectangle Frame { get; set; }

        internal AndroidApplicationContext CurrentContext { get; private set; }

        public virtual void AnimateTouch(MotionEvent e)
        {
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            CurrentContext = (AndroidApplicationContext)applicationContext;
        }
        #endregion

        internal abstract void SetBackground(Drawable drawable);

        protected sealed override void RefreshView()
        {
            CurrentContext.CurrentNativeScreen.RelayoutView();
        }
    }

}