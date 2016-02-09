using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    public abstract class CustomSwipeLayout : Control<UIScrollView>, ILayoutableContainer, IValidatable
    {
        private const double SwipeEps = 0.001;
        protected readonly ISwipeBehaviour Behaviour;
        protected readonly ILayoutableContainerBehaviour<Control> ContainerBehaviour;
        private UIImage _backgroundImage;
        private float _previousX;
        private float _previousY;
        private float _startX;
        private float _startY;

        public CustomSwipeLayout()
        {
            Behaviour = ControlsContext.Current.CreateSwipeBehaviour(Scroll);
            ContainerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
            Scrollable = true;
        }

        public int Index
        {
            get { return Behaviour.Index; }
            set { Behaviour.Index = value; }
        }

        public int Percent
        {
            get { return (int)Math.Round(Behaviour.Percent * 100); }
            set { Behaviour.Percent = (float)value / 100; }
        }

        public string Alignment
        {
            get { return Behaviour.Alignment; }
            set { Behaviour.Alignment = value; }
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
            _view = new UIScrollView();
            _view.ShowsVerticalScrollIndicator = false;
            _view.ShowsHorizontalScrollIndicator = false;
            _view.Bounces = false;
            _view.DecelerationRate = UIScrollView.DecelerationRateFast;
            _view.Delegate = new ScrollViewDelegate(this);
            _view.ScrollEnabled = Scrollable;

            CreateChildrens();
        }

        #region IContainer implementation

        public void AddChild(object obj)
        {
            Insert(ContainerBehaviour.Childrens.Count, obj);
        }

        public object[] Controls
        {
            get { return ContainerBehaviour.Childrens.ToArray(); }
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
            if (ContainerBehaviour.Childrens.Count > index)
            {
                UIView view = ContainerBehaviour.Childrens[index].View;
                if (view != null)
                    view.RemoveFromSuperview();
            }

            ContainerBehaviour.Withdraw(index);
        }

        public void Inject(int index, string xml)
        {
            ContainerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
            foreach (Control control in ContainerBehaviour.Childrens)
                if (control.View == null)
                {
                    control.CreateView();
                    _view.AddSubview(control.View);
                }
        }
        #endregion

        #region IValidatable implementation

        public bool Validate()
        {
            bool result = true;

            foreach (Control control in ContainerBehaviour.Childrens)
            {
                var validatable = control as IValidatable;
                if (validatable != null)
                    result &= validatable.Validate();
            }

            return result;
        }

        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color, background image, borders
            _backgroundImage = stylesheet.SetBackgroundSettings(this);

            // size to content by background
            IBound boundByBackground = GetBoundByImage(styleBound, maxBound, _backgroundImage);
            IBound bound = LayoutChildren(stylesheet, boundByBackground, maxBound);

            SetupAlignOffset(bound);
            _view.ContentOffset = GetContentOffset(Behaviour.OffsetByIndex);

            Behaviour.IndexChanged += HandleIndexChanged;

            return bound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, background image, borders
                _backgroundImage = helper.SetBackgroundSettings(this);
                if (_backgroundImage != null)
                    DrawBackgroundImage();
            }

            // size to content by background
            IBound boundByBackground = GetBoundByImage(styleBound, maxBound, _backgroundImage);
            IBound bound = LayoutChildren(CurrentStyleSheet, boundByBackground, maxBound);

            SetupAlignOffset(bound);
            _view.ContentOffset = GetContentOffset(Behaviour.OffsetByIndex);

            return bound;
        }

        protected override void Dismiss()
        {
            foreach (Control control in ContainerBehaviour.Childrens)
                control.DismissView();

            DisposeField(ref _backgroundImage);

            Behaviour.IndexChanged -= HandleIndexChanged;
            
            if (_view.Delegate != null)
                _view.Delegate.Dispose();
            _view.Delegate = null;
        }

        protected override void OnSetFrame()
        {
            if (_backgroundImage != null)
                DrawBackgroundImage();
        }

        protected void AlignOffset(out float startOffset, out float endOffset)
        {
            startOffset = 0;
            endOffset = 0;
            if (Behaviour.CenterAlignment && Behaviour.Borders.Count > 1)
            {
                float[] borders = Behaviour.Borders.ToArray();
                startOffset = (Behaviour.ScrollingArea - (borders[1] - borders[0])) / 2;
                endOffset = (Behaviour.ScrollingArea - (borders[borders.Length - 1] - borders[borders.Length - 2])) / 2;
            }
        }

        protected void Scroll(float offset)
        {
            if (_view != null)
                _view.SetContentOffset(GetContentOffset(offset), true);
        }

        protected abstract IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound);

        protected abstract void OnScrollEnded(float startX, float startY);

        protected abstract void SetupAlignOffset(IBound bound);

        protected abstract PointF GetContentOffset(float offset);

        private void HandleIndexChanged()
        {
            if (OnSwipe != null)
                OnSwipe.Execute();
        }

        private void HandleDraggingStarted()
        {
            CloseModalWindows();
            EndEditing();

            _previousX = _startX;
            _previousY = _startY;
            if (_view != null)
            {
                _startX = _view.ContentOffset.X;
                _startY = _view.ContentOffset.Y;
            }
        }

        private void HandleDraggingEnded()
        {
            if (_view != null)
                if (Math.Abs(_startX - _view.ContentOffset.X) > SwipeEps
                    || Math.Abs(_startY - _view.ContentOffset.Y) > SwipeEps)
                {
                    OnScrollEnded(_startX, _startY);
                }
        }

        private void HandleDecelerationEnded()
        {
            if (_view != null)
            {
                if (Math.Abs(_startX - _view.ContentOffset.X) > SwipeEps
                    || Math.Abs(_startY - _view.ContentOffset.Y) > SwipeEps)
                    OnScrollEnded(_startX, _startY);
                else
                    OnScrollEnded(_previousX, _previousY);
            }
        }

        private void DrawBackgroundImage()
        {
            UIImage img = null;
            try
            {
                UIGraphics.BeginImageContext(_view.Frame.Size);
                _backgroundImage.Draw(_view.Bounds);
                img = UIGraphics.GetImageFromCurrentImageContext();
                if (img != null)
                    _view.BackgroundColor = UIColor.FromPatternImage(img);
            }
            finally
            {
                UIGraphics.EndImageContext();
                if (img != null)
                    img.Dispose();
            }
        }

        private class ScrollViewDelegate : UIScrollViewDelegate
        {
            private readonly CustomSwipeLayout _swipeLayout;

            public ScrollViewDelegate(CustomSwipeLayout swipeLayout)
            {
                _swipeLayout = swipeLayout;
            }

            public override void DraggingStarted(UIScrollView scrollView)
            {
                _swipeLayout.HandleDraggingStarted();
            }

            public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
            {
                _swipeLayout.HandleDraggingEnded();
            }

            public override void DecelerationEnded(UIScrollView scrollView)
            {
                _swipeLayout.HandleDecelerationEnded();
            }
        }
    }
}