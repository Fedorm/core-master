using System.Linq;
using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using System;
using System.Collections.Generic;

namespace BitMobile.Controls
{
    [Synonym("body")]
    // ReSharper disable ClassNeverInstantiated.Global, UnusedMember.Global, MemberCanBePrivate.Global
    internal class Screen : CustomLayout, IScreen, ICustomStyleSheet
    {
        private IGesturable _gestureHolder;

        public Screen(BaseScreen activity)
            : base(activity)
        {
        }

        public bool GestureHolded()
        {
            return _gestureHolder != null;
        }

        public bool GestureHoldedExcept(IGesturable control)
        {
            return _gestureHolder != null && _gestureHolder != control;
        }

        public bool TryHoldGesture(IGesturable control)
        {
            if (_gestureHolder == null)
            {
                _gestureHolder = control;
                return true;
            }
            return _gestureHolder == control;
        }

        public ActionHandlerEx OnLoading
        {
            set
            {
                if (value != null)
                    value.Execute();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ActionHandlerEx OnLoad { get; set; }

        public override void AddChild(object obj)
        {
            if (Childrens.Count == 0)
                base.AddChild(obj);
            else
                throw new Exception("Only one child is allowed in Screen container");
        }

        public override View CreateView()
        {
            base.CreateView();

            _view.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            _view.FocusableInTouchMode = true;

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            var bound = new Bound(BitBrowserApp.Current.Width, BitBrowserApp.Current.Height);

            base.Apply(stylesheet, bound, bound);

			//background color
            _view.SetBackgroundColor(Android.Graphics.Color.White);

            if (OnLoad != null)
                OnLoad.Execute();

            Frame = new Rectangle(0, 0, bound);

            return bound;
        }

        #region IScreen

        public void ExitEditMode()
        {
            _activity.HideSoftInput();
        }
        #endregion

        #region ICustomStyleSheet

        public string StyleSheet { get; set; }
        #endregion

        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            base.View_TouchingInvoke(sender, e);

            if (e.Event.Action == MotionEventActions.Down)
                CleadGestureHolder();

        }

        protected override void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            base.View_TouchInvoke(sender, e);

            if (!(_activity.CurrentFocus is Android.Widget.EditText))
                ExitEditMode();

            if (e.Event.Action == MotionEventActions.Up
                || e.Event.Action == MotionEventActions.Cancel)
                CleadGestureHolder();

            e.Handled = true;
        }

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            return LayoutBehaviour.Screen(stylesheet, this, Childrens[0], maxBound);
        }

        private void CleadGestureHolder()
        {
            _gestureHolder = null;
        }
    }
}