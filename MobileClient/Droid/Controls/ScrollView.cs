using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Runtime;
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
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "ScrollView")]
    [Synonym("sv")]
    // ReSharper disable ClassNeverInstantiated.Global, RedundantExtendsListEntry, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public class ScrollView : Control<ScrollView.ScrollViewNative>, ILayoutableContainer, IValidatable, IPersistable
    {
        private readonly ILayoutableContainerBehaviour<Control> _containerBehaviour;
        private int _scrollIndex;
        private int _scrollTop;
        private bool _inTouch;

        public ScrollView(BaseScreen activity)
            : base(activity)
        {
            _containerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
        }

        public int Index
        {
            get
            {
                if (_view != null)
                    _scrollIndex = _view.FirstVisiblePosition;
                return _scrollIndex;
            }
            set
            {
                _scrollIndex = value;
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
            _view = new ScrollViewNative(Activity, this);
            _view.SetSelector(Android.Resource.Color.Transparent);
            _view.TouchEvent += View_TouchInvoke;
            _view.ScrollStateChanged += View_ScrollStateChanged;
            _view.ScrollingCacheEnabled = false;
            _view.Divider = null;
            _view.CacheColorHint = Color.Transparent;
            _view.DescendantFocusability = DescendantFocusability.AfterDescendants;

            if (_scrollIndex > 0)
                SetIndex();
        }

        #region IContainer

        public void AddChild(object obj)
        {
            Insert(_containerBehaviour.Childrens.Count, obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _containerBehaviour.Childrens.ToArray();
            }
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
            _containerBehaviour.Withdraw(index);
        }

        public void Inject(int index, string xml)
        {
            _containerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
            // todo: обработать
        }

        #endregion

        #region IValidatable

        public bool Validate()
        {
            return _containerBehaviour.Childrens.OfType<IValidatable>()
                .Aggregate(true, (current, validatable) => current & validatable.Validate());
        }

        #endregion

        #region IPersistable

        public object GetState()
        {
            return new[] { Index, _scrollTop };
        }

        public void SetState(object state)
        {
            var arr = state as int[];
            if (arr != null)
            {
                _scrollIndex = arr[0];
                _scrollTop = arr[1];
            }
        }
        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color
            _view.SetBackgroundColor(stylesheet.Helper.BackgroundColor(this).ToColorOrTransparent());

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
                    _view.SetBackgroundColor(backgroundColor.ToColorOrTransparent());
            }

            foreach (Control control in _containerBehaviour.Childrens)
                if (control.View != null)
                    ApplyChild(control, styleBound);

            _view.InvalidateViews();
            _view.Invalidate();

            return styleBound;
        }

        protected override void Dismiss()
        {            
            foreach (Control control in _containerBehaviour.Childrens)
                control.DismissView();
            base.Dismiss();
        }

        private View GetView(int position)
        {
            if (position <= _containerBehaviour.Childrens.Count)
            {
                Control control = _containerBehaviour.Childrens[position];
                if (control.View == null)
                    try
                    {
                        control.CreateView();
                        ApplyChild(control, Frame.Bound);
                    }
                    catch (Exception e)
                    {
                        CurrentContext.HandleException(e);
                    }

                return control.View;
            }
            throw new Exception("Position less than count");
        }

        private void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {            
            e.Handled = false;
        }

        private void ApplyChild(Control control, IBound rootBound)
        {
            float parentWidth = rootBound.Width;
            float parentHeight = rootBound.Height;

            IStyleSheetHelper style = CurrentStyleSheet.Helper;
            float width = style.Width(control, parentWidth);
            float height = style.Height(control, parentHeight);

            IBound bound = control.ApplyStyles(CurrentStyleSheet, StyleSheetContext.Current.CreateBound(width, height), StyleSheetContext.Current.CreateBound(parentWidth, float.PositiveInfinity));
            control.Frame = ControlsContext.Current.CreateRectangle(0, 0, bound);

            control.View.LayoutParameters = new AbsListView.LayoutParams(control.Frame.Width.Round(), control.Frame.Height.Round());
        }

        private void View_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (e.ScrollState == ScrollState.TouchScroll)
            {
                View currentFocus = Activity.CurrentFocus;
                if (currentFocus != null)
                    currentFocus.ClearFocus();
            }
        }

        private void SetIndex()
        {
            int realIndex;
            int count = _containerBehaviour.Childrens.Count;
            if (_scrollIndex >= count)
                realIndex = count > 0 ? count - 1 : 0;
            else if (_scrollIndex < 0)
                realIndex = 0;
            else
                realIndex = _scrollIndex;

            if (_view.IsShown)
                _view.SmoothScrollToPosition(realIndex);
            else
                _view.SetSelectionFromTop(realIndex, _scrollTop);

            _scrollTop = 0;
        }

        private void OnScrollInvoke(int index)
        {
            _scrollIndex = _view.FirstVisiblePosition;
            if (_containerBehaviour.Childrens.Count > index && index >= 0)
            {
                View view = _containerBehaviour.Childrens[index].View;
                if (view != null)
                    _scrollTop = view.Top;

                ScrollIndex = index;
                if (OnScroll != null)
                    OnScroll.Execute();
            }
            else
            {
                Console.WriteLine("Bad index: " + index);
            }
        }

        public sealed class ScrollViewNative : ListView, View.IOnTouchListener
        {
            private readonly ScrollView _scrollView;
            public event EventHandler<TouchEventArgs> TouchEvent;

            bool _disposed;

            public ScrollViewNative(Android.Content.Context activity, ScrollView scrollView)
                : base(activity)
            {
                _scrollView = scrollView;
                var adapter = new CustomAdapter(scrollView);
                Adapter = adapter;
                SetOnTouchListener(this);
            }

            // ReSharper disable once UnusedMember.Global
            public ScrollViewNative(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
                // if GC collect ScrollView, dalvik creates .NET proxy
            }

            public override bool OnInterceptTouchEvent(MotionEvent ev)
            {
                Screen scr = _scrollView.CurrentContext.CurrentNativeScreen;
                return base.OnInterceptTouchEvent(ev) && !scr.GestureHolded();
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                base.OnTouchEvent(e);
                TouchEvent.Execute(this, e);
                return true;
            }


            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (!_disposed)
                {
                    Adapter.Dispose();
                    Adapter = null;

                    _disposed = true;
                }
            }

            class CustomAdapter : Java.Lang.Object, IListAdapter
            {
                readonly ScrollView _scrollView;

                public CustomAdapter(ScrollView scrollView)
                {
                    _scrollView = scrollView;
                }

                // ReSharper disable once UnusedMember.Local
                public CustomAdapter(IntPtr handle, JniHandleOwnership transfer)
                    : base(handle, transfer)
                {
                    // if GC collect ScrollView, dalvik creates .NET proxy
                }

                public int Count
                {
                    get { return _scrollView._containerBehaviour.Childrens.Count; }
                }

                public Java.Lang.Object GetItem(int position)
                {
                    return _scrollView._containerBehaviour.Childrens[position].ToString();
                }

                public long GetItemId(int position)
                {
                    return position;
                }

                public View GetView(int position, View convertView, ViewGroup parent)
                {
                    // convertView is _controller.View, but this feature provide reloading of styles (size)
                    // if (convertView != null)
                    //     return convertView;
                    return _scrollView.GetView(position);
                }

                public bool AreAllItemsEnabled()
                {
                    return true;
                }

                public bool IsEnabled(int position)
                {
                    return true;
                }

                public int GetItemViewType(int position)
                {
                    return position;
                }

                public bool HasStableIds
                {
                    get { return true; }
                }

                public bool IsEmpty
                {
                    get { return _scrollView._containerBehaviour.Childrens.Count == 0; }
                }

                public void RegisterDataSetObserver(Android.Database.DataSetObserver observer)
                {
                }

                public void UnregisterDataSetObserver(Android.Database.DataSetObserver observer)
                {
                }

                public int ViewTypeCount
                {
                    get
                    {
                        int result = _scrollView._containerBehaviour.Childrens.Count;
                        return result > 1 ? result : 1;
                    }
                }
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Up:
                    case MotionEventActions.Cancel:
                        _scrollView._inTouch = false;
                        break;
                    default:
                        if (!_scrollView._inTouch)
                        {
                            _scrollView._inTouch = true;
                            var location = new int[2];
                            GetLocationOnScreen(location);
                            int x = (int)e.RawX - location[0];
                            int y = (int)e.RawY - location[1];
                            bool found = false;
                            int index;
                            for (index = 0; index < _scrollView._containerBehaviour.Childrens.Count; index++)
                            {
                                Control child = _scrollView._containerBehaviour.Childrens[index];

                                if (child.View != null)
                                    using (var rect = new Rect())
                                    {
                                        child.View.GetHitRect(rect);
                                        if (rect.Contains(x, y))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                            }

                            if (found)
                                _scrollView.OnScrollInvoke(index);
                        }
                        break;
                }
                return false;
            }
        }
    }
}
