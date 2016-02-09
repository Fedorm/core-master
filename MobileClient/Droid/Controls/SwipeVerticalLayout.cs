using System;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "SwipeVerticalLayout")]
    [Synonym("svl")]
    // ReSharper disable once UnusedMember.Global
    class SwipeVerticalLayout : CustomSwipeLayout
    {
        float _startY;
        float _lastY;

        public SwipeVerticalLayout(BaseScreen activity)
            : base(activity)
        {
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            IBound bound = ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Vertical(ContainerBehaviour.Childrens, styleBound, maxBound, out borders, true);

            if (ContainerBehaviour.Childrens.Count > 0)
                Behavour.SetBorders(borders);

            Behavour.ScrollingArea = bound.Height;
            return bound;
        }

        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            Screen scr = CurrentContext.CurrentNativeScreen;

            if (Scrollable && !scr.GestureHoldedExcept(this))
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        _startY = _view.ScrollY;
                        _lastY = e.Event.GetY();
                        ScrollPerGesture = 0;
                        break;
                    case MotionEventActions.Move:
                        float y = e.Event.GetY();
                        float delta = _lastY - y;
                        if (Math.Abs(delta) > 0.001)
                        {
                            ScrollPerGesture += delta;

                            delta = CutBorders(delta, _view.ScrollY);

                            if (Math.Abs(ScrollPerGesture) >= TouchSlop)
                                Scrolled = scr.TryHoldGesture(this);

                            if (Scrolled)
                                _view.ScrollBy(0, (int)delta);

                            _lastY = y;
                        }
                        break;
                }
        }

        protected override void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            base.View_TouchInvoke(sender, e);

            if (Scrolled)
                if (e.Event.Action == MotionEventActions.Up
                    || e.Event.Action == MotionEventActions.Cancel)
                {
                    float? offset = Behavour.HandleSwipe(_view.ScrollY, _startY);
                    if (offset != null)
                    {
                        Scroll(offset.Value);
                        Scrolled = false;
                        ScrollPerGesture = 0;
                        e.Handled = true;
                    }
                }
        }

        protected override void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            base.View_LayoutInvoke(changed, l, t, r, b);
            float offset = Behavour.OffsetByIndex;
            _view.ScrollTo(0, (int)offset);
        }

        protected override void Scroll(float offset)
        {
            if (_view != null && Layouted)
            {
                float delta = offset - _view.ScrollY;
                float duration = Math.Abs(delta) * 500 / _view.Height;
                Scroller.StartScroll(0, _view.ScrollY, 0, (int)delta, (int)duration);
                _view.PostInvalidate();   
            }
        }
    }
}