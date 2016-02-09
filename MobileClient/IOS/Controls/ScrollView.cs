using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Application.Translator;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [Synonym("sv")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "ScrollView")]
    public class ScrollView : Control<ScrollView.NativeTableView>, ILayoutableContainer, IValidatable, IPersistable
    {
        private readonly ILayoutableContainerBehaviour<Control> _containerBehaviour;
        private bool _hideOverlaysAfterScrolling = true;
        private DateTime _scrollAnimationFinished;
        private Action _scrollEndedCallback;
        private int _scrollIndex;
        private PointF? _scrollOffset;
        private IStyleSheet _stylesheet;

        public ScrollView()
        {
            _containerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
        }

        public int Index
        {
            get
            {
                if (_view != null && _containerBehaviour.Childrens.Count > 0)
                {
                    NSIndexPath path = _view.IndexPathsForVisibleRows[0];
                    _scrollIndex = path.Row;
                }

                return _scrollIndex;
            }
            set
            {
                _scrollIndex = value;
                _scrollOffset = null;
                if (_view != null)
                    SetIndex();
            }
        }

        public IActionHandlerEx OnScroll { get; set; }

        public int ScrollIndex { get; private set; }

        // ReSharper disable once InconsistentNaming
        public object append(string xml)
        {
            Inject(_containerBehaviour.Childrens.Count, xml);
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
            _view = new NativeTableView(this);
            _view.Source = new NativeTableViewSource(this);
            _view.SeparatorStyle = UITableViewCellSeparatorStyle.None;
        }

        public bool ScrollTo(int index, Action callback)
        {
            NSIndexPath[] rows = _view.IndexPathsForVisibleRows;
            if (rows.Length > 0)
            {
                int minIndex = rows[0].Row;
                int maxIndex = rows[rows.Length - 1].Row;
                if (index > minIndex && index < maxIndex)
                {
                    callback();
                    return true;
                }
            }

            if ((DateTime.Now - _scrollAnimationFinished).TotalMilliseconds > 400)
            {
                _scrollEndedCallback = callback;
                _hideOverlaysAfterScrolling = false;
                Index = index;
                return true;
            }
            return false;
        }

        #region IContainer

        public void AddChild(object obj)
        {
            Insert(_containerBehaviour.Childrens.Count, obj);
        }

        public object[] Controls
        {
            get { return _containerBehaviour.Childrens.ToArray(); }
        }

        public object GetControl(int index)
        {
            return _containerBehaviour.Childrens[index];
        }

        #endregion
        
        #region ILayoutableContainer

        public void Insert(int index, object obj)
        {
            _containerBehaviour.Insert(index, obj);
        }

        public void Withdraw(int index)
        {
            if (_containerBehaviour.Childrens.Count > index)
            {
                UIView view = _containerBehaviour.Childrens[index].View;
                if (view != null)
                    view.RemoveFromSuperview();
            }

            _containerBehaviour.Withdraw(index);
        }

        public void Inject(int index, string xml)
        {
            _containerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
        }
        #endregion

        #region IValidatable 

        public bool Validate()
        {
            bool result = true;

            foreach (Control control in _containerBehaviour.Childrens)
            {
                var validatable = control as IValidatable;
                if (validatable != null)
                    result &= validatable.Validate();
            }

            return result;
        }

        #endregion

        #region IPersistable 

        public object GetState()
        {
            if (_view != null)
                return _view.ContentOffset;
            return null;
        }

        public void SetState(object state)
        {
            if (state is PointF)
                _scrollOffset = (PointF)state;
        }

        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            _stylesheet = stylesheet;

            // background color
            _view.BackgroundColor = stylesheet.Helper.BackgroundColor(this).ToColorOrClear();

            return styleBound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color
                IBackgroundColor backgroundColor;
                if (helper.TryGet(out backgroundColor))
                    _view.BackgroundColor = backgroundColor.ToColorOrClear();
            }

            foreach (Control control in _containerBehaviour.Childrens)
                if (control.View != null)
                    ApplyChild(control, styleBound);

            Index = ScrollIndex;
            _view.ReloadData();

            return styleBound;
        }

        protected override void Dismiss()
        {
            DismissControls();

            UITableViewSource source = _view.Source;
            if (source != null)
            {                
                _view.Source = null;
                source.Dispose();
            }
        }

        protected override void OnSetFrame()
        {
            if (_scrollOffset.HasValue)
                _view.SetContentOffset(_scrollOffset.Value, false);
            else
                SetIndex();
        }

        private void DismissControls()
        {
            foreach (Control control in _containerBehaviour.Childrens)
                control.DismissView();
        }

        private void SetIndex()
        {
            if (_containerBehaviour.Childrens.Count > 0)
            {
                int realIndex;
                if (_scrollIndex >= _containerBehaviour.Childrens.Count)
                    realIndex = _containerBehaviour.Childrens.Count > 0 ? _containerBehaviour.Childrens.Count - 1 : 0;
                else if (_scrollIndex < 0)
                    realIndex = 0;
                else
                    realIndex = _scrollIndex;
                
                bool isShown = _view.Window != null;
                NSIndexPath path = NSIndexPath.FromRowSection(realIndex, 0);
                try
                {
                    _view.ScrollToRow(path, UITableViewScrollPosition.Top, isShown);
                }
                catch (Exception e)
                {
                    CurrentContext.HandleException(new NonFatalException(D.ERROR, "Exception in ScrollView.", e));
                }
                
            }
        }

        private void HideKeyboard()
        {
            if (_hideOverlaysAfterScrolling)
            {
                CloseModalWindows();
                EndEditing();
            }
        }

        private UITableViewCell GetCell(int position)
        {
            IControl<UIView> control = GetView(position);

            var cell = new UITableViewCell();
            cell.ContentView.AddSubview(control.View);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.BackgroundColor = _view.BackgroundColor;

            return cell;
        }

        private IControl<UIView> GetView(int position)
        {
            Control control = _containerBehaviour.Childrens[position];
            if (control.View == null)
            {
                try
                {
                    control.CreateView();
                    ApplyChild(control, Frame.Bound);
                }
                catch (Exception e)
                {
                    CurrentContext.HandleException(e);
                }
            }
            return control;
        }

        private void ApplyChild(Control control, IBound rootBound)
        {
            float parentWidth = rootBound.Width;
            float parentHeight = rootBound.Height;

            IStyleSheetHelper style = _stylesheet.Helper;
            float w = style.Width(control, parentWidth);
            float h = style.Height(control, parentHeight);

            IStyleSheetContext context = StyleSheetContext.Current;
            IBound bound = control.ApplyStyles(_stylesheet, context.CreateBound(w, h),
                context.CreateBound(parentWidth, float.MaxValue));

            control.Frame = ControlsContext.Current.CreateRectangle(0, 0, bound);
        }

        private void OnScrollInvoke(int index)
        {
            ScrollIndex = index;

            if (OnScroll != null)
                OnScroll.Execute();
        }

        public class NativeTableView : UITableView
        {
            private PointF _lastPoint;
            private ScrollView _scrollView;

            public NativeTableView(ScrollView scrollView)
            {
                _scrollView = scrollView;
            }

            public override UIView HitTest(PointF point, UIEvent uievent)
            {
                UIView hitView = base.HitTest(point, uievent);
                if (_scrollView == null)
                    return hitView;
                try
                {
                    if (hitView != null && point != _lastPoint && hitView.Superview != null &&
                        hitView.Superview.Superview != null && hitView.Superview.Superview.Superview != null)
                    {
                        UIView view = hitView;

                        while (!(view is UITableViewCell))
                        {
                            view = view.Superview;
                            if (view == null)
                                return hitView;
                        }
                        view = ((UITableViewCell)view).ContentView.Subviews[0];

                        int index = -1;
                        for (int i = 0; i < _scrollView._containerBehaviour.Childrens.Count; i++)
                        {
                            UIView cview = _scrollView._containerBehaviour.Childrens[i].View;
                            if (view.Equals(cview))
                            {
                                index = i;
                                break;
                            }
                        }

                        _scrollView.OnScrollInvoke(index);
                        _lastPoint = point;
                    }
                    return hitView;
                }
                catch (Exception e)
                {
                    LogManager.Logger.Error(e.ToString(), false);
                    return hitView;
                }
            }

            public override void MovedToWindow()
            {
                base.MovedToWindow();

                // Scroll to 1 px execute redraw missing (invisible) items in TableView
                SetContentOffset(new PointF(ContentOffset.X, ContentOffset.Y + 1), true);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _scrollView = null;
            }
        }

        public class NativeTableViewSource : UITableViewSource
        {
            private ScrollView _scrollView;

            public NativeTableViewSource(ScrollView scrollView)
            {
                _scrollView = scrollView;
            }

            public override void Scrolled(UIScrollView scrollView)
            {
                if (_scrollView == null)
                    return;
                _scrollView.HideKeyboard();
            }

            public override void ScrollAnimationEnded(UIScrollView scrollView)
            {
                if (_scrollView == null)
                    return;

                if (_scrollView._scrollEndedCallback != null)
                {
                    _scrollView._scrollEndedCallback();
                    _scrollView._scrollEndedCallback = null;

                    _scrollView._hideOverlaysAfterScrolling = true;
                }

                _scrollView._scrollAnimationFinished = DateTime.Now;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                if (_scrollView == null)
                    return 0;
                return _scrollView._containerBehaviour.Childrens.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (_scrollView == null)
                    return new UITableViewCell();
                return _scrollView.GetCell(indexPath.Row);
            }

            public override float EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableView.AutomaticDimension;
            }

            public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (_scrollView == null)
                    return 0;
                return _scrollView.GetView(indexPath.Row).Frame.Height;
            }

            protected override void Dispose(bool disposing)
            {
                _scrollView = null;

                base.Dispose(disposing);
            }
        }
    }
}