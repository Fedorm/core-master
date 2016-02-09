using System;
using BitMobile.Controls;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.Utilities.LogManager;
using BitMobile.IOS;

namespace BitMobile.UI
{
	public abstract class Control<T> : Control
		where T : UIView
	{
		protected T _view;
		protected bool _visible = true;
		bool _disposed = false;

		public Control ()
		{
		}

		public override bool Visible {
			get {
				return _visible;
			}
			set {
				_visible = value;
				if (_view != null)
					_view.Hidden = !_visible;
			}
		}

		[NonLog]
		public override UIView View {
			get {
				return _view;
			}
		}

		[NonLog]
		internal T ViewInternal {
			get {
				return _view;
			}
		}

		public override Rectangle Frame {
			get {
				return new Rectangle (_view.Frame.Left, _view.Frame.Top, _view.Frame.Width, _view.Frame.Height);
			}
			set {
				_view.Frame = new System.Drawing.RectangleF (value.Left, value.Top, value.Width, value.Height);
				OnSetFrame ();
			}
		}

		public override Bound Apply (StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			if (_view == null)
				throw new NullReferenceException ("cannot apply styles: View is null.");			

			_view.Hidden = !_visible;

			return styleBound;
		}

		protected virtual void OnSetFrame()
		{
		}

		protected void CloseModalWindows ()
		{
			DatePicker.CancelCurrent ();
		}

		protected void EndEditing ()
		{
			((Screen)BitMobile.Application.ApplicationContext.Context.CurrentScreen.Screen).View.EndEditing (true);
		}

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (_view != null)
					_view.Dispose ();
				_view = null;

				_disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}

