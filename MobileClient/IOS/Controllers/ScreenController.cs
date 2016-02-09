using System;
using System.Drawing;
using BitMobile.Application;
using BitMobile.Application.BusinessProcess;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class ScreenController : UIViewController
    {
        private float _lastOffset;
        private NSObject _ko1, _ko2;

        public ScreenController(UIView view)
        {
            if (view != null)
                View = view;

            if (Version.Parse(UIDevice.CurrentDevice.SystemVersion).Major >= 7)
                EdgesForExtendedLayout = UIRectEdge.None;

            HackFieldForDateTimePicker = new UITextField();
        }

        public static UITextField HackFieldForDateTimePicker { get; private set; }

        public UIView GetFirstResponder()
        {
            return FindFirstResponder(View);
        }

        public override void ViewWillAppear(bool animated)
        {
            _ko1 = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardDidShow);
            _ko2 = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardDidHide);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_ko1);
            NSNotificationCenter.DefaultCenter.RemoveObserver(_ko2);
            _lastOffset = 0;
        }

        private void KeyboardDidShow(NSNotification notification)
        {
            HandleKeyboardAppearing(notification, false);
        }

        private void KeyboardDidHide(NSNotification notification)
        {
            HandleKeyboardAppearing(notification, true);
        }

        private UIView FindFirstResponder(UIView v)
        {
            if (v.IsFirstResponder)
                return v;

            foreach (UIView sv in v.Subviews)
            {
                UIView r = FindFirstResponder(sv);
                if (r != null)
                    return r;
            }

            return null;
        }

        protected virtual float GetPositionToMove()
        {
            float position = 0;
            UIView responder = GetFirstResponder();
            if (responder != null)
            {
                RectangleF r = responder.ConvertRectToView(responder.Bounds, null);
                position = r.Y + r.Height;
            }
            return position;
        }

        private void HandleKeyboardAppearing(NSNotification notification, bool movedDown)
        {
            if (movedDown)
            {
                float offset = _lastOffset*-1;

                MoveView(notification, offset);

                _lastOffset = 0;
            }
            else if (_lastOffset == 0)
            {
                var frame = (NSValue) notification.UserInfo.ObjectForKey(UIKeyboard.FrameEndUserInfoKey);
                float offset = frame.RectangleFValue.Height;
                float screenHeight = UIScreen.MainScreen.Bounds.Height;

                float position = GetPositionToMove();

                if (position > screenHeight - offset)
                {
                    offset = offset + position - screenHeight;

                    MoveView(notification, offset);

                    _lastOffset = offset;
                }
                else
                {
                    _lastOffset = 0;
                }
            }
        }

        private void MoveView(NSNotification notification, float offset)
        {
            if (offset != 0)
            {
                var animationDuration =
                    (NSValue) notification.UserInfo.ObjectForKey(UIKeyboard.AnimationDurationUserInfoKey);
                double duration = ((NSNumber) animationDuration).DoubleValue;

                UIView.BeginAnimations(null);
                UIView.SetAnimationDuration(duration);
                RectangleF rect = View.Frame;
                rect.Y -= offset;
                rect.Height += offset;
                View.Frame = rect;
                UIView.CommitAnimations();
            }
        }

        #region respond to shaking

        public override bool CanBecomeFirstResponder
        {
            get { return true; }
        }

        public override void MotionEnded(UIEventSubtype motion, UIEvent evt)
        {
            if (motion == UIEventSubtype.MotionShake)
            {
                // Do your application-specific shake response here...
                if (ApplicationContext.Current != null && ApplicationContext.Current.ValueStack != null)
                {
                    string workflow = ApplicationContext.Current.Workflow.Name;
                    BusinessProcessContext.Current.GlobalEventsController.OnApplicationShake(workflow);
                }
            }
        }

        #endregion
    }
}