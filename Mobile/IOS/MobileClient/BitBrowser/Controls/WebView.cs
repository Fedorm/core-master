using System;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using MonoTouch.Foundation;
using BitMobile.UI;

namespace BitMobile.Controls
{
	public class WebView :Control<WebView.NativeWebView>
	{
		bool _disposed = false;

		public WebView ()
		{
		}

		public virtual string Url { get; set; }

		public override UIView CreateView ()
		{
			_view = new NativeWebView ();
			_view.MovedToWindowEvent += HandleMovedToWindowEvent;
			_view.TouchesBeganEvent += HandleTouchesBeganEvent;

			return _view;
		}

		protected virtual void LoadPage ()
		{
			_view.LoadRequest (new NSUrlRequest (new NSUrl (Url)));
		}

		void HandleMovedToWindowEvent ()
		{
			LoadPage ();
		}

		void HandleTouchesBeganEvent (NSSet arg1, UIEvent arg2)
		{
			CloseModalWindows ();
		}

		public class NativeWebView: UIWebView
		{
			public event Action MovedToWindowEvent;
			public event Action<NSSet, UIEvent> TouchesBeganEvent = delegate {};

			public override void MovedToWindow ()
			{
				base.MovedToWindow ();

				if (MovedToWindowEvent != null)
					MovedToWindowEvent ();
			}

			public override void TouchesBegan (NSSet touches, UIEvent evt)
			{
				base.TouchesBegan (touches, evt);
				TouchesBeganEvent (touches, evt);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (_view != null)
					_view.MovedToWindowEvent -= HandleMovedToWindowEvent;
				_disposed = true;
			}
			base.Dispose (disposing);
		}
	}
}

