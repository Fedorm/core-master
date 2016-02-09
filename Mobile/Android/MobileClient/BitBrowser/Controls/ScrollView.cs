using System.Linq;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using System;
using System.Collections.Generic;
using Common.Controls;

namespace BitMobile.Controls
{
    [Synonym("sv")]
    // ReSharper disable ClassNeverInstantiated.Global, RedundantExtendsListEntry, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public class ScrollView : Control<ScrollView.ScrollViewNative>
        , IContainer, IValidatable, IPersistable, IApplicationContextAware
    {
        readonly List<IControl<View>> _controls = new List<IControl<View>>();
        StyleSheet.StyleSheet _stylesheet;
        private int _scrollIndex;
        private int _scrollTop;
        private ApplicationContext _applicationContext;
        private bool _inTouch;

        public ScrollView(BaseScreen activity)
            : base(activity)
        {
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

        public ActionHandlerEx OnScroll { get; set; }

        public int ScrollIndex { get; private set; }

        public override View CreateView()
        {
            _view = new ScrollViewNative(_activity, this);
            _view.SetSelector(Android.Resource.Color.Transparent);
            _view.TouchEvent += View_TouchInvoke;
            _view.ScrollStateChanged += View_ScrollStateChanged;
            _view.ScrollingCacheEnabled = false;
            _view.Divider = null;
            _view.CacheColorHint = Android.Graphics.Color.Transparent;

            if (_scrollIndex > 0)
                SetIndex();

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            _stylesheet = stylesheet;

            // background color
            _view.SetBackgroundColor(stylesheet
                .GetHelper<StyleHelper>()
                .ColorOrTransparent<BackgroundColor>(this));

            return styleBound;
        }

        #region IContainer

        public void AddChild(object obj)
        {
            var control = obj as IControl<View>;
            if (control == null)
                throw new Exception(string.Format("Incorrect child: {0}", obj));

            _controls.Add(control);
            control.Parent = this;
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _controls.ToArray();
            }
        }

        public object GetControl(int index)
        {
            return _controls[index];
        }
        #endregion

        #region IValidatable

        public bool Validate()
        {
            return _controls.OfType<IValidatable>().Aggregate(true, (current, validatable) => current & validatable.Validate());
        }

        #endregion

        #region IPersistable

        public object GetState()
        {
            int index = Index;
            int top = 0;
            if (_controls.Count > index)
            {
                View view = _controls[index].View;
                if (view != null)
                    top = view.Top;
            }

            return new[] { Index, top };
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

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (ApplicationContext)applicationContext;
        }
        #endregion

        private View GetView(int position)
        {
            if (position <= _controls.Count)
            {
                IControl<View> control = _controls[position];
                if (control.View == null)
                {
                    control.CreateView();
                    LayoutChild(control);
                    
                    control.View.LayoutParameters =
                        new AbsListView.LayoutParams(control.Frame.Width.Round(), control.Frame.Height.Round());
                }

                return control.View;
            }
            throw new Exception("Position less than count");
        }

        void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            _activity.HideSoftInput();
            e.Handled = false;
        }

        void LayoutChild(IControl<View> control)
        {
            float controlWidth = Frame.Width;
            float controlHeight = Frame.Height;

            float width = _stylesheet.GetHelper<StyleHelper>().Width(control, controlWidth);
            float height = _stylesheet.GetHelper<StyleHelper>().Height(control, controlHeight);

            LayoutBehaviour.InitializeImageContainer(_stylesheet, control, ref width, ref height);
            
            Bound bound = control.Apply(_stylesheet, new Bound(width, height), new Bound(width, float.PositiveInfinity));

            control.Frame = new Rectangle(0, 0, bound);
        }

        void View_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (e.ScrollState == ScrollState.TouchScroll)
            {
                View currentFocus = _activity.CurrentFocus;
                if (currentFocus != null)
                    currentFocus.ClearFocus();
            }
        }

        void SetIndex()
        {
            int realIndex;
            if (_scrollIndex >= _controls.Count)
                realIndex = _controls.Count > 0 ? _controls.Count - 1 : 0;
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

        void OnScrollInvoke(int index)
        {
            ScrollIndex = index;
            if (OnScroll != null)
                OnScroll.Execute();
        }

        public sealed class ScrollViewNative : ListView, View.IOnTouchListener
        {
            private readonly ScrollView _controller;
            public event EventHandler<TouchEventArgs> TouchEvent;

            bool _disposed;

            public ScrollViewNative(Android.Content.Context activity, ScrollView controller)
                : base(activity)
            {
                _controller = controller;
                var adapter = new CustomAdapter(controller);
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
                Screen scr = _controller._applicationContext.CurrentNativeScreen;
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
                readonly ScrollView _controller;

                public CustomAdapter(ScrollView controller)
                {
                    _controller = controller;
                }

                // ReSharper disable once UnusedMember.Local
                public CustomAdapter(IntPtr handle, JniHandleOwnership transfer)
                    : base(handle, transfer)
                {
                    // if GC collect ScrollView, dalvik creates .NET proxy
                }

                public int Count
                {
                    get { return _controller._controls.Count; }
                }

                public Java.Lang.Object GetItem(int position)
                {
                    return _controller._controls[position].ToString();
                }

                public long GetItemId(int position)
                {
                    return position;
                }

                public View GetView(int position, View convertView, ViewGroup parent)
                {
                    if (convertView != null)
                        return convertView;
                    return _controller.GetView(position);
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
                    get { return _controller._controls.Count == 0; }
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
                        int result = _controller._controls.Count;
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
                        _controller._inTouch = false;
                        break;
                    default:
                        if (!_controller._inTouch)
                        {
                            _controller._inTouch = true;
                            var location = new int[2];
                            GetLocationOnScreen(location);
                            int x = (int)e.RawX - location[0];
                            int y = (int)e.RawY - location[1];
                            int index = _controller._controls.FindIndex(val =>
                            {
                                if (val.View != null)
                                    using (var rect = new Rect())
                                    {
                                        val.View.GetHitRect(rect);
                                        return rect.Contains(x, y);
                                    }
                                return false;
                            });
                            _controller.OnScrollInvoke(index);
                        }
                        break;
                }
                return false;
            }
        }
    }
}
