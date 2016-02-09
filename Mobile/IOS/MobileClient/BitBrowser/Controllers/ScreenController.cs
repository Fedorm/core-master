using System;
using System.Collections.Generic;
using MonoTouch.UIKit;
using BitMobile.Actions;
using MonoTouch.Foundation;
using System.Drawing;

namespace BitMobile.IOS
{
	public class ScreenController : UIViewController
	{
		NSObject ko1, ko2 = null;
		float _lastOffset;

		public ScreenController (UIView view) : // Вот тут нужно просто выводить не вью, а контрол. А из него в LoadView вызывать CreateView
			base ()
		{
			if (view != null)
				this.View = view;

			if (Version.Parse (UIDevice.CurrentDevice.SystemVersion).Major >= 7)
				this.EdgesForExtendedLayout = UIRectEdge.None;

			HackFieldForDateTimePicker = new UITextField ();


		}

		public static UITextField HackFieldForDateTimePicker { get; private set; }

		public UIView GetFirstResponder ()
		{
			return this.FindFirstResponder (this.View);
		}

		public override void ViewWillAppear (bool animated)
		{
			ko1 = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, KeyboardDidShow);
			ko2 = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, KeyboardDidHide);
			base.ViewWillAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);	
			NSNotificationCenter.DefaultCenter.RemoveObserver (ko1);
			NSNotificationCenter.DefaultCenter.RemoveObserver (ko2);
			_lastOffset = 0;
		}

		private void KeyboardDidShow (NSNotification notification)
		{
			HandleKeyboardAppearing (notification, false);	
		}

		private void KeyboardDidHide (NSNotification notification)
		{
			HandleKeyboardAppearing (notification, true);
		}

		private UIView FindFirstResponder (UIView v)
		{
			if (v.IsFirstResponder)
				return v;

			foreach (UIView sv in v.Subviews) {
				UIView r = FindFirstResponder (sv);
				if (r != null)
					return r;
			}

			return null;
		}

		#region respond to shaking

		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}

		public override void MotionEnded (UIEventSubtype motion, UIEvent evt)
		{
			if (motion == UIEventSubtype.MotionShake) {
				// Do your application-specific shake response here...
				if (BitMobile.Application.ApplicationContext.Context != null && BitMobile.Application.ApplicationContext.Context.ValueStack != null) {
					string workflow = BitMobile.Application.ApplicationContext.Context.Workflow.Name;
					BitMobile.Application.ApplicationContext.Context.ValueStack.TryCallScript ("Events", "OnApplicationShake", workflow);
				}
			}
		}

		#endregion

		protected virtual float GetPositionToMove ()
		{
			float position = 0;
			var responder = GetFirstResponder ();
			if (responder != null) {
				RectangleF r = responder.ConvertRectToView (responder.Bounds, null);
				position = r.Y + r.Height;
			}
			return position;
		}

		void HandleKeyboardAppearing (NSNotification notification, bool movedDown)
		{
			if (movedDown) {
				float offset = _lastOffset * -1;

				MoveView (notification, offset);

				_lastOffset = 0;
			} else if (_lastOffset == 0) {
				NSValue frame = (NSValue)notification.UserInfo.ObjectForKey (UIKeyboard.FrameEndUserInfoKey);
				float offset = frame.RectangleFValue.Height;
				float screenHeight = UIScreen.MainScreen.Bounds.Height;

				var position = GetPositionToMove ();

				if (position > screenHeight - offset) {
					offset = offset + position - screenHeight;								

					MoveView (notification, offset);

					_lastOffset = offset;	

				} else {
					_lastOffset = 0;
				}
			}
		}

		void MoveView (NSNotification notification, float offset)
		{
			if (offset != 0) {

				NSValue animationDuration = (NSValue)notification.UserInfo.ObjectForKey (UIKeyboard.AnimationDurationUserInfoKey);
				double duration = ((NSNumber)animationDuration).DoubleValue;

				UIView.BeginAnimations (null);
				UIView.SetAnimationDuration (duration);
				RectangleF rect = this.View.Frame;
				rect.Y -= offset;
				rect.Height += offset;
				this.View.Frame = rect;
				UIView.CommitAnimations ();
			}
		}
	}
}

