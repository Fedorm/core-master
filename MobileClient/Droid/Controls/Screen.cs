using System;
using System.Collections.Generic;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Screen")]
    [Synonym("body")]
    // ReSharper disable ClassNeverInstantiated.Global, UnusedMember.Global, MemberCanBePrivate.Global
    internal class Screen : CustomLayout, IScreen, ICustomStyleSheet
    {
        private IGesturable _gestureHolder;
        // In some devices View.Scroll in SwipeLayout executes OnLayout in root container.
        private bool _layoutAllowed = true;

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

        public IActionHandlerEx OnLoading
        {
            set
            {
                if (value != null)
                    value.Execute();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IActionHandlerEx OnLoad { get; set; }

        public override void AddChild(object obj)
        {
            if (ContainerBehaviour.Childrens.Count == 0)
                base.AddChild(obj);
            else
                throw new Exception("Only one child is allowed in Screen container");
        }

        public override void CreateView()
        {
            base.CreateView();

            _view.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            _view.FocusableInTouchMode = true;
        }

        public void RelayoutView()
        {
            if (_view != null)
            {
                _layoutAllowed = true;
                _view.Invalidate();
                _view.RequestLayout();
            }
        }

        #region IScreen

        public void ExitEditMode()
        {
            Activity.HideSoftInput();
        }
        #endregion

        #region ICustomStyleSheet

        public string StyleSheet { get; set; }
        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            var bound = StyleSheetContext.Current.CreateBound(BitBrowserApp.Current.Width, BitBrowserApp.Current.Height);
            base.Apply(stylesheet, bound, bound);

            //background color
            _view.SetBackgroundColor(Android.Graphics.Color.White);

            if (OnLoad != null)
                OnLoad.Execute();

            Frame = ControlsContext.Current.CreateRectangle(0, 0, bound);

            return bound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            var bound = StyleSheetContext.Current.CreateBound(BitBrowserApp.Current.Width, BitBrowserApp.Current.Height);
            base.ReApply(styles, bound, bound);

            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);
            if (styles.Count > 0)
            {
                // background color
                IBackgroundColor backgroundColor;
                if (helper.TryGet(out backgroundColor))
                    _view.SetBackgroundColor(backgroundColor.ToColorOrTransparent());
            }

            return bound;
        }

        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            base.View_TouchingInvoke(sender, e);

            if (e.Event.Action == MotionEventActions.Down)
                CleadGestureHolder();

        }

        protected override void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            base.View_TouchInvoke(sender, e);

            if (e.Event.Action == MotionEventActions.Up
                || e.Event.Action == MotionEventActions.Cancel)
                CleadGestureHolder();

            e.Handled = true;
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            if (ContainerBehaviour.Childrens.Count == 1)
                return ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this).Screen(ContainerBehaviour.Childrens[0], maxBound);
            return styleBound;
        }

        protected override void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            if (_layoutAllowed)
            {
                _layoutAllowed = false;
                base.View_LayoutInvoke(changed, l, t, r, b);
            }
        }

        private void CleadGestureHolder()
        {
            _gestureHolder = null;
        }
    }
}