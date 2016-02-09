using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using BitMobile.Controls;
using BitMobile.UI;
using System.Threading.Tasks;
using System.Threading;

namespace BitMobile.IOS
{
	public class TabOrderManager: IDisposable
	{
		public static TabOrderManager Current { get; private set; }

		public static void Create (ApplicationContext context)
		{
			if (Current != null)
				Current.Dispose ();
			
			Current = new TabOrderManager (context);
		}

		ApplicationContext _context;
		List<Control> _items = new List<Control> ();
		UIView _accessory;
		UIButton _back;
		UIButton _next;
		UIButton _cancel;
		bool _inProgress;

		private TabOrderManager (ApplicationContext context)
		{
			_context = context;
		}

		public void Add(Control control)
		{
			_inProgress = false;
			_items.Add (control);
		}

		public void AttachAccessory (EditText editText)
		{
			editText.ViewInternal.InputAccessoryView = CreateAccessory ();
		}

		public void AttachAccessory (MemoEdit memoEdit)
		{
			memoEdit.ViewInternal.InputAccessoryView = CreateAccessory ();
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			_items.Clear ();
			_items = null;
			_context = null;
			if (_accessory != null) {
				_accessory.Dispose ();
				_accessory = null;
				_back.Dispose ();
				_back = null;
				_next.Dispose ();
				_next = null;
				_cancel.Dispose ();
				_cancel = null;
			}
		}

		#endregion

		UIView CreateAccessory ()
		{
			var app = UIScreen.MainScreen.ApplicationFrame;
			const float btnWidth = 40;
			const float btnHeight = 40;
			const float margin = 5;

			_accessory = new UIView (new RectangleF (0, 0, 1, btnHeight));			
			_accessory.BackgroundColor = UIColor.White;
			_accessory.ClipsToBounds = true;
			_accessory.Layer.BorderWidth = 1 / UIScreen.MainScreen.Scale;
			_accessory.Layer.BorderColor = UIColor.LightGray.CGColor;

			_back = new UIButton (UIButtonType.System);
			_back.Frame = new RectangleF (app.Width - btnWidth - margin - btnWidth - margin - btnWidth, 0, btnWidth, btnHeight);
			_back.SetTitle ("<", UIControlState.Normal);
			_back.TouchUpInside += HandleBack;
			_accessory.AddSubview (_back);

			_next = new UIButton (UIButtonType.System);
			_next.Frame = new RectangleF (app.Width - btnWidth - margin - btnWidth, 0, btnWidth, btnHeight);
			_next.SetTitle (">", UIControlState.Normal);
			_next.TouchUpInside += HandleNext;
			_accessory.AddSubview (_next);

			_cancel = new UIButton (UIButtonType.System);
			_cancel.Frame = new RectangleF (app.Width - btnWidth, 0, btnWidth, btnHeight);
			_cancel.SetTitle ("x", UIControlState.Normal);
			_cancel.TouchUpInside += HandleCancel;
			_accessory.AddSubview (_cancel);

			return _accessory;
		}

		void HandleBack (object sender, EventArgs e)
		{
			int next = -1;
			UIView current = _context.GetFirstResponder ();
			for (int i = _items.Count - 1; i >= 0; i--) {
				UIView view = _items [i].View;
				if (view != null && view.Equals (current)) {
					next = i - 1;
					break;
				}
			}

			ChangeResponder (next, current);
		}

		void HandleNext (object sender, EventArgs e)
		{
			int next = -1;
			UIView current = _context.GetFirstResponder ();
			for (int i = 0; i < _items.Count; i++){
				UIView view = _items [i].View;
				if (view != null && view.Equals (current)) {
					next = i + 1;
					break;
				}
			}

			ChangeResponder (next, current);
		}

		void HandleCancel (object sender, EventArgs e)
		{
			_context.GetFirstResponder ().ResignFirstResponder ();
		}

		void ChangeResponder (int next, UIView current)
		{
			if (!_inProgress && next != -1) {
				_inProgress = true;
				if (next < _items.Count && next >= 0) {
					bool result = ScrollParentToView (_items [next], () => {
						current.ResignFirstResponder ();
						_items [next].View.BecomeFirstResponder ();
						_inProgress = false;
					});
					if (!result)
						_inProgress = false;
				} else
					_inProgress = false;
			}
		}

		bool ScrollParentToView (Control control, Action callback)
		{
			var parent = control.Parent as IContainer;
			if (parent == null) {
				callback ();
				return true;
			}

			var scrollView = parent as ScrollView;
			if (scrollView != null) {
				int index = -1;
				for (int i = 0; i < scrollView.Controls.Length; i++)
					if (scrollView.Controls [i].Equals (control)) {
						index = i;
						break;
					}
				if (index >= 0) {
					return scrollView.ScrollTo (index, callback);
				}
			}

			var swipeLayout = parent as CustomSwipeLayout;
			if (swipeLayout != null) {
				// todo: доделать
			}

			return ScrollParentToView ((Control)parent, callback);
		}
	}
}

