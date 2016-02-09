using Android.Content;
using Android.Runtime;
using Android.Views;
using System;

namespace BitMobile.Droid.UI
{
    public class CustomViewGroup : ViewGroup
    {
        private bool _layoutExecuted = false;

        public event LayoutHandler LayoutEvent;

        public event MeasureHandler MeasureEvent;

        public event EventHandler<TouchEventArgs> TouchingEvent;

        public event EventHandler<TouchEventArgs> TouchEvent;

        public event EventHandler ComputeScrollEvent;

        public CustomViewGroup(Context activity)
            : base(activity)
        {
        }

        public CustomViewGroup(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (changed && !_layoutExecuted)
            {
                _layoutExecuted = true;
                if (LayoutEvent != null)
                    LayoutEvent(changed, l, t, r, b);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            if (MeasureEvent != null)
                MeasureEvent(widthMeasureSpec, heightMeasureSpec);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            TouchingEvent.Execute(this, e);
            base.DispatchTouchEvent(e);
            TouchEvent.Execute(this, e);
            return true;
        }

        public override void ComputeScroll()
        {
            if (ComputeScrollEvent != null)
                ComputeScrollEvent(this, new EventArgs());
        }

        public class LayoutParams : Android.Views.ViewGroup.LayoutParams
        {
            public int Left { get; private set; }

            public int Top { get; private set; }

            public int Right
            {
                get { return Left + base.Width; }
            }

            public int Bottom
            {
                get { return Top + base.Height; }
            }

            public LayoutParams(int width, int height, int left, int top)
                : base(width, height)
            {
                Left = left;
                Top = top;
            }
        }
    }


}