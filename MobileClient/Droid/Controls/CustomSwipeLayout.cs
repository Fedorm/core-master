using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using BitMobile.Application.Controls;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    abstract class CustomSwipeLayout : Control<CustomViewGroup>, ILayoutableContainer, IValidatable, IGesturable
    {
        protected readonly ISwipeBehaviour Behavour;
        protected readonly ILayoutableContainerBehaviour<Control> ContainerBehaviour;
        protected Scroller Scroller;
        protected int TouchSlop;
        protected float ScrollPerGesture;
        protected bool Scrolled;
        protected bool Layouted;

        protected CustomSwipeLayout(BaseScreen activity)
            : base(activity)
        {
            Behavour = ControlsContext.Current.CreateSwipeBehaviour(Scroll);
            ContainerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);

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

        public IActionHandlerEx OnSwipe { get; set; }

        // ReSharper disable once InconsistentNaming
        public object append(string xml)
        {
            Inject(ContainerBehaviour.Childrens.Count, xml);
            return this;
        }

        // ReSharper disable once InconsistentNaming
        public object prepend(string xml)
        {
            Inject(0, xml);
            return this;
        }

        public override void CreateView()
        {
            using (ViewConfiguration configuration = ViewConfiguration.Get(Activity))
                TouchSlop = configuration.ScaledTouchSlop;
            Scroller = new Scroller(Activity);

            _view = new CustomViewGroup(Activity);
            _view.TouchEvent += View_TouchInvoke;
            _view.TouchingEvent += View_TouchingInvoke;
            _view.LayoutEvent += View_LayoutInvoke;
            _view.MeasureEvent += View_MeasureInvoke;
            _view.ComputeScrollEvent += View_ComputeScrollEvent;
            _view.Clickable = false;

            CreateChildrens();
        }

        #region IContainer

        public virtual void AddChild(object obj)
        {
            Insert(ContainerBehaviour.Childrens.Count, obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return ContainerBehaviour.Childrens.ToArray();
            }
        }

        public object GetControl(int index)
        {
            return ContainerBehaviour.Childrens[index];
        }

        #endregion

        #region ILayoutableContainer

        public void Insert(int index, object obj)
        {
            ContainerBehaviour.Insert(index, obj);
        }

        public void Withdraw(int index)
        {
            ContainerBehaviour.Withdraw(index);
            _view.RemoveViewAt(index);
        }

        public void Inject(int index, string xml)
        {
            ContainerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
            for (int i = 0; i < ContainerBehaviour.Childrens.Count; i++)
            {
                Control control = ContainerBehaviour.Childrens[i];
                if (control.View == null)
                {
                    control.CreateView();
                    _view.AddView(control.View, i);
                }
            }
        }

        #endregion

        #region IValidatable

        public bool Validate()
        {
            return ContainerBehaviour.Childrens.OfType<IValidatable>()
                .Aggregate(true, (current, validatable) => current && validatable.Validate());
        }

        #endregion

        #region IGesturable

        public GestureType SupportedGesture { get; protected set; }

        #endregion

        protected sealed override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color, background image, borders
            using (var background = stylesheet.Background(this, styleBound))
                SetBackground(background);

            Behavour.IndexChanged += BehavourOnIndexChanged;

            IBound bound = GetBoundByBackgroud(styleBound, maxBound);
            return LayoutChildren(stylesheet, bound, maxBound);
        }

        protected sealed override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, background image, borders
                Drawable background;
                if (helper.BackgroundChanged(CurrentStyleSheet, Frame.Bound, out background))
                    using (background)
                        SetBackground(background);
            }

            IBound bound = GetBoundByBackgroud(styleBound, maxBound);
            return LayoutChildren(CurrentStyleSheet, bound, maxBound);
        }

        protected abstract IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound);

        protected abstract void Scroll(float offset);

        protected virtual void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected virtual void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected sealed override void Dismiss()
        {            
            foreach (var control in ContainerBehaviour.Childrens)
                control.DismissView();

            DisposeField(ref Scroller);
            base.Dismiss();
        }

        protected virtual void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            Layouted = true;
            foreach (IControl<View> control in ContainerBehaviour.Childrens)
            {
                var left = (int)Math.Round(control.Frame.Left);
                var top = (int)Math.Round(control.Frame.Top);
                var right = (int)Math.Round(control.Frame.Right);
                var bottom = (int)Math.Round(control.Frame.Bottom);

                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                control.View.LayoutParameters = new CustomViewGroup.LayoutParams(width, height, left, top);
                control.View.Layout(left, top, right, bottom);
                control.View.RequestLayout();
            }
        }

        /// <summary>
        /// Blocking bounce
        /// </summary>
        /// <param name="delta">Delta for gesture</param>
        /// <param name="scroll">Curernt scroll</param>
        /// <returns>Changed delta</returns>
        protected float CutBorders(float delta, int scroll)
        {
            float scrollingSpace = Behavour.Borders.Last();
            if (scrollingSpace < Behavour.ScrollingArea)
                return 0;

            float offsetStart = 0;
            float offsetEnd = 0;
            if (Behavour.CenterAlignment && ContainerBehaviour.Childrens.Count > 0)
            {
                float[] borders = Behavour.Borders.ToArray();
                int last = borders.Length - 1;
                offsetStart = (Behavour.ScrollingArea - (borders[1] - borders[0])) / 2;
                offsetEnd = (Behavour.ScrollingArea - (borders[last] - borders[last - 1])) / 2;
            }

            if (scroll + delta + offsetStart < 0)
                delta = -1 * scroll - offsetStart;

            if (scroll + delta + Behavour.ScrollingArea - offsetEnd > scrollingSpace)
                delta = scrollingSpace + offsetEnd - scroll - Behavour.ScrollingArea;
            return delta;
        }

        private void BehavourOnIndexChanged()
        {
            if (OnSwipe != null)
                OnSwipe.Execute();
        }

        void View_MeasureInvoke(int widthMeasureSpec, int heightMeasureSpec)
        {
            foreach (IControl<View> control in ContainerBehaviour.Childrens)
            {
                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                // RedundantNameQualifier to fix compile error
                // ReSharper disable RedundantNameQualifier
                int wspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
                int hspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);
                // ReSharper restore RedundantNameQualifier
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