using System;
using System.Collections.Generic;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.UI;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebView")]
    public class WebView : Control<WebView.NativeWebView>
    {
        public virtual string Url { get; set; }

        public override void CreateView()
        {
            _view = new NativeWebView();
            _view.MovedToWindowEvent += HandleMovedToWindowEvent;
            _view.TouchesBeganEvent += HandleTouchesBeganEvent;
        }

        protected virtual void LoadPage()
        {
            _view.LoadRequest(new NSUrlRequest(new NSUrl(Url)));
        }

        private void HandleMovedToWindowEvent()
        {
            LoadPage();
        }

        private void HandleTouchesBeganEvent(NSSet arg1, UIEvent arg2)
        {
            CloseModalWindows();
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            return styleBound;
        }

        protected override void Dismiss()
        {
            _view.MovedToWindowEvent -= HandleMovedToWindowEvent;
        }

        public class NativeWebView : UIWebView
        {
            public event Action MovedToWindowEvent;
            public event Action<NSSet, UIEvent> TouchesBeganEvent = delegate { };

            public override void MovedToWindow()
            {
                base.MovedToWindow();

                if (MovedToWindowEvent != null)
                    MovedToWindowEvent();
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                TouchesBeganEvent(touches, evt);
            }
        }
    }
}