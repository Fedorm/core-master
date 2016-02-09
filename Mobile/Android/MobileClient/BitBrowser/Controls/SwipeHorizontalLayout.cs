using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using System;
using System.Linq;

namespace BitMobile.Controls
{
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

        protected override Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            Bound bound = LayoutBehaviour.Horizontal(stylesheet, this, Childrens, styleBound, maxBound, true);

            if (Childrens.Count > 0)
            {
                Behavour.Borders.Add(Childrens[0].Frame.Left);
                foreach (IControl<View> control in Childrens)
                    Behavour.Borders.Add(control.Frame.Right);
            }
            Behavour.ScrolledMeasure = bound.Width;
            return bound;
        }

        protected override void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            Screen scr = ApplicationContext.CurrentNativeScreen;

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

                            if (_view.ScrollX + delta < 0)
                                delta = -1 * _view.ScrollX;

                            float scrolledWidth = Behavour.Borders.Last();
                            if (_view.ScrollX + delta + Behavour.ScrolledMeasure > scrolledWidth)
                                delta = scrolledWidth - _view.ScrollX - Behavour.ScrolledMeasure;

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
                    float offset = Behavour.HandleSwipe(_view.ScrollX, _startX, _view.ScrollX);
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
            _view.ScrollBy((int)offset, 0);
        }

        protected override void Scroll(float offset)
        {
            if (_view != null && Layouted)
            {
                float delta = offset - _view.ScrollX;
                float duration = Math.Abs(delta) * 500 / _view.Width;
                Scroller.StartScroll(_view.ScrollX, 0, (int)delta, 0, (int)duration);
                _view.Invalidate();

                base.Scroll(offset);
            }
        }
    }
}