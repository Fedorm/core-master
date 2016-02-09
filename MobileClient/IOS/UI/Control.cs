using BitMobile.Application;
using BitMobile.Common.Controls;
using BitMobile.IOS;
using MonoTouch.UIKit;

namespace BitMobile.UI
{
    public abstract class Control : CustomControl<UIView>, IApplicationContextAware
    {
        public enum TouchEventType
        {
            Begin,
            End,
            Cancel
        }

        public abstract bool Visible { get; set; }

        protected IOSApplicationContext CurrentContext { get; private set; }

        public virtual void AnimateTouch(TouchEventType touch)
        {
        }

        public void SetApplicationContext(object applicationContext)
        {
            CurrentContext = (IOSApplicationContext)applicationContext;
        }
    }
}