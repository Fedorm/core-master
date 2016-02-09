using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Widget;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid.UI;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    abstract class CustomSwipeLayout : Control<CustomViewGroup>, IApplicationContextAware, IContainer, IImageContainer, IValidatable, IGesturable
    {
        protected ApplicationContext ApplicationContext;
        protected List<IControl<View>> Childrens = new List<IControl<View>>();
        protected readonly SwipeBehaviour Behavour;
        protected readonly Scroller Scroller;
        protected float ScrollPerGesture;
        protected readonly int TouchSlop;
        protected bool Scrolled;
        protected bool Layouted;
        bool _disposed;

        protected CustomSwipeLayout(BaseScreen activity)
            : base(activity)
        {
            using (ViewConfiguration configuration = ViewConfiguration.Get(activity))
                TouchSlop = configuration.ScaledTouchSlop;
            Scroller = new Scroller(activity);

            Behavour = new SwipeBehaviour(Scroll);

            SupportedGesture = GestureType.Any;

            Scrollable = true;
        }

        public int Index
        {
            get { return Behavour.Index; }
            set { Behavour.Index = value; }
        }

        public int Percent
        {
            get { return (int)Math.Round(Behavour.Percent * 100); }
            set { Behavour.Percent = (float)value / 100; }
        }

        public string Alignment
        {
            get { return Behavour.Alignment; }
            set { Behavour.Alignment = value; }
        }

        public bool Scrollable { get; set; }

        public ActionHandlerEx OnSwipe { get; set; }

        public override View CreateView()
        {
            _view = new CustomViewGroup(_activity);
            _view.TouchEvent += View_TouchInvoke;
            _view.TouchingEvent += View_TouchingInvoke;
            _view.LayoutEvent += View_LayoutInvoke;
            _view.MeasureEvent += View_MeasureInvoke;
            _view.ComputeScrollEvent += View_ComputeScrollEvent;
            _view.Clickable = false;

            foreach (IControl<View> control in Childrens)
                _view.AddView(control.CreateView());

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // background color, background image, borders
            var background = style.Background(this, ApplicationContext);
            _view.SetBackgroundDrawable(background);

            return LayoutChildren(stylesheet, styleBound, maxBound);
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            ApplicationContext = (ApplicationContext)applicationContext;
        }
        #endregion

        #region IContainer

        public virtual void AddChild(object obj)
        {
            var control = obj as IControl<View>;
            if (control == null)
                throw new Exception(string.Format("Incorrect child: {0}", obj));

            Childrens.Add(control);
            control.Parent = this;
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return Childrens.ToArray();
            }
        }

        public object GetControl(int index)
        {
            return Childrens[index];
        }
        #endregion

        #region IImageContainer

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public bool InitializeImage(StyleSheet.StyleSheet stylesheet)
        {
            return stylesheet.GetHelper<StyleHelper>().InitializeImageContainer(this, ApplicationContext);
        }
        #endregion

        #region IValidatable

        public bool Validate()
        {
            return Childrens
                .OfType<IValidatable>()
                .Aggregate(true, (current, validatable) => current && validatable.Validate());
        }

        #endregion

        #region IGesturable

        public GestureType SupportedGesture { get; protected set; }

        #endregion

        protected abstract Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound);

        protected virtual void Scroll(float offset)
        {
            if (OnSwipe != null)
                OnSwipe.Execute();
        }

        protected virtual void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected virtual void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ApplicationContext = null;
                }

                foreach (var control in Childrens)
                    control.Dispose();
                Childrens.Clear();
                Childrens = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected virtual void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            Layouted = true;
            foreach (IControl<View> control in Childrens)
            {
                if (control.Frame == Rectangle.Empty)
                    throw new Exception(string.Format(
                        "Invalid frame: parent {0}, control {1}", this, control));

                var left = (int)Math.Round(control.Frame.Left);
                var top = (int)Math.Round(control.Frame.Top);
                var right = (int)Math.Round(control.Frame.Right);
                var bottom = (int)Math.Round(control.Frame.Bottom);

                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                control.View.LayoutParameters = new CustomViewGroup.LayoutParams(width, height, left, top);
                control.View.Layout(left, top, right, bottom);
            }
        }

        void View_MeasureInvoke(int widthMeasureSpec, int heightMeasureSpec)
        {
            foreach (IControl<View> control in Childrens)
            {
                if (control.Frame == Rectangle.Empty)
                    throw new Exception(string.Format(
                        "Invalid frame: parent {0}, control {1}", this, control));

                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                int wspec = View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
                int hspec = View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);
                control.View.Measure(wspec, hspec);
            }
        }

        void View_ComputeScrollEvent(object sender, EventArgs e)
        {
            if (Scroller.ComputeScrollOffset())
            {
                _view.ScrollTo(Scroller.CurrX, Scroller.CurrY);
                _view.PostInvalidate();
            }
        }
    }
}