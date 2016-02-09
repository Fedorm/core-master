using System;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "SwipeHorizontalLayout")]
    [Synonym("shl")]
    // ReSharper disable once UnusedMember.Global
    class SwipeHorizontalLayout : CustomSwipeLayout
    {
        float _startX;
        float _lastX;

        public SwipeHorizontalLayout(BaseScreen activity)
            : base(activity)
        {
            SupportedGesture = GestureType.Horizontal;
        }

        protected override IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            float[] borders;
            IBound bound = ControlsContext.Current.CreateLayoutBehaviour(stylesheet, this)
                .Horizontal(ContainerBehaviour.Childrens, styleBound, maxBound, out borders, true);

            if (ContainerBehaviour.Childrens.Count > 0)
                Behavour.SetBorders(borders);

            Behavour.ScrollingArea = bound.Width;
            return bound;
        }
        
        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            Screen scr = CurrentContext.CurrentNativeScreen;

            if (Scrollable && !scr.GestureHoldedExcept(this))
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        _startX = _view.ScrollX;
                        _lastX = e.Event.GetX();
                        ScrollPerGesture = 0;
                        break;
                    case MotionEventActions.Move:
                        float x = e.Event.GetX();
                        float delta = _lastX - x;
                        if (Math.Abs(delta) > 0.001)
                        {
                            ScrollPerGesture += delta;

                            delta = CutBorders(delta, _view.ScrollX);

                            if (Math.Abs(ScrollPerGesture) >= TouchSlop)
                                Scrolled = scr.TryHoldGesture(this);

                            if (Scrolled)
                                _view.ScrollBy((int)delta, 0);

                            _lastX = x;
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
                    float? offset = Behavour.HandleSwipe(_view.ScrollX, _startX);
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
            _view.ScrollTo((int)offset, 0);
        }

        protected override void Scroll(float offset)
        {
            if (_view != null && Layouted)
            {
                float delta = offset - _view.ScrollX;
                float duration = Math.Abs(delta) * 500 / _view.Width;
                Scroller.StartScroll(_view.ScrollX, 0, (int)delta, 0, (int)duration);
                _view.PostInvalidate();
            }
        }
    }
}