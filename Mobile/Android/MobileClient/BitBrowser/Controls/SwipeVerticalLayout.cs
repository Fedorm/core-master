using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using System;
using System.Linq;

namespace BitMobile.Controls
{
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

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
           Bound bound = LayoutBehaviour.Vertical(stylesheet, this, Childrens, styleBound, maxBound, true);

            if (Childrens.Count > 0)
            {
                Behavour.Borders.Add(Childrens[0].Frame.Top);
                foreach (IControl<View> control in Childrens)
                    Behavour.Borders.Add(control.Frame.Bottom);
            }
            Behavour.ScrolledMeasure = bound.Height;
            return bound;
        }

        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            Screen scr = ApplicationContext.CurrentNativeScreen;

            if (Scrollable && !scr.GestureHoldedExcept(this))
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        _startY = _view.ScrollX;
                        _lastY = e.Event.GetY();
                        ScrollPerGesture = 0;
                        break;
                    case MotionEventActions.Move:
                        float y = e.Event.GetY();
                        float delta = _lastY - y;
                        if (Math.Abs(delta) > 0.001)
                        {
                            ScrollPerGesture += delta;

                            if (_view.ScrollY + delta < 0)
                                delta = -1 * _view.ScrollY;

                            float scrolledheight = Behavour.Borders.Last();
                            if (_view.ScrollY + delta + Behavour.ScrolledMeasure > scrolledheight)
                                delta = scrolledheight - _view.ScrollY - Behavour.ScrolledMeasure;

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
                    float offset = Behavour.HandleSwipe(_startY, e.Event.GetY(), _view.ScrollY);
                    Scroll(offset);
                    Scrolled = false;
                    ScrollPerGesture = 0;
                    e.Handled = true;
                }
        }

        protected override void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            base.View_LayoutInvoke(changed, l, t, r, b);
            float offset = Behavour.OffsetByIndex;
            _view.ScrollBy(0, (int)offset);
        }

        protected override void Scroll(float offset)
        {
            if (_view != null && Layouted)
            {
                float delta = offset - _view.ScrollY;
                float duration = Math.Abs(delta) * 500 / _view.Height;
                Scroller.StartScroll(0, _view.ScrollY, 0, (int)delta, (int)duration);
                _view.Invalidate();

                base.Scroll(offset);
            }
        }
    }
}