using System.Linq;
using Android.Graphics.Drawables;
using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using System;
using System.Collections.Generic;

namespace BitMobile.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public abstract class CustomLayout : Control<CustomViewGroup>, IApplicationContextAware, IContainer, IImageContainer, IValidatable
    {
        ApplicationContext _applicationContext;
        bool _disposed;
        Drawable _mainBackground;
        bool _pressed;
        Drawable _selectedBackground;
        bool _clickable;
        ActionHandler _onClickAction;
        ActionHandlerEx _onClick;
        protected List<Control> Childrens = new List<Control>();

        protected CustomLayout(BaseScreen activity)
            : base(activity)
        {
        }

        public ActionHandler OnClickAction
        {
            get { return _onClickAction; }
            set
            {
                _onClickAction = value;
                SetClickable(value);
            }
        }

        public ActionHandlerEx OnClick
        {
            get { return _onClick; }
            set
            {
                _onClick = value;
                SetClickable(value);
            }
        }

        public String OnEvent
        {
            get
            {
                return _onEvent;
            }
            set
            {
                BitBrowserApp.Current.SubscribeEvent(value, InvokeClickAction);
                _onEvent = value;
            }
        }
        string _onEvent;

        public override View CreateView()
        {
            _view = new CustomViewGroup(_activity);
            _view.TouchEvent += View_TouchInvoke;
            _view.TouchingEvent += View_TouchingInvoke;
            _view.LayoutEvent += View_LayoutInvoke;
            _view.MeasureEvent += View_MeasureInvoke;
            _view.Clickable = _clickable;

            foreach (IControl<View> control in Childrens)
                _view.AddView(control.CreateView());

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // background color, background image, borders
            var background = style.Background(this, _applicationContext);
            _view.SetBackgroundDrawable(background);

            // selected color
            Android.Graphics.Color? selectedColor = style.Color<SelectedColor>(this);
            if (selectedColor != null)
                _selectedBackground = style.ColorWithBorders(this, selectedColor.Value);

            return LayoutChildren(stylesheet, styleBound, maxBound);
        }

        public override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_selectedBackground != null)
                    {
                        _mainBackground = View.Background;
                        _view.SetBackgroundDrawable(_selectedBackground);
                    }

                    foreach (var control in Childrens)
                        control.AnimateTouch(e);

                    _view.Invalidate();

                    _pressed = true;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_pressed)
                    {
                        if (_mainBackground != null)
                            _view.SetBackgroundDrawable(_mainBackground);

                        foreach (var control in Childrens)
                            control.AnimateTouch(e);

                        _mainBackground = null;
                        _pressed = false;
                    }
                    break;
            }
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (ApplicationContext)applicationContext;
        }
        #endregion

        #region IContainer

        public virtual void AddChild(object obj)
        {
            var control = obj as Control;
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
            return stylesheet.GetHelper<StyleHelper>().InitializeImageContainer(this, _applicationContext);
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

        protected abstract Bound LayoutChildren(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound);

        protected virtual void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected virtual void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            if (_clickable)
            {
                e.Handled = true;
                if (_onClickAction != null || _onClick != null)
                {
                    if (e.Event.Action == MotionEventActions.Down && !EditableExist(this))
                        _activity.HideSoftInput();

                    if (_pressed && e.Event.Action == MotionEventActions.Up)
                        if (!_applicationContext.CurrentNativeScreen.GestureHolded())
                            InvokeClickAction();

                    AnimateTouch(e.Event);
                }
            }
            else
                e.Handled = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _applicationContext = null;
                }

                foreach (var control in Childrens)
                    control.Dispose();
                Childrens.Clear();
                Childrens = null;

                if (_mainBackground != null)
                {
                    _mainBackground.Dispose();
                    _mainBackground = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
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

        bool InvokeClickAction()
        {
            if (_onClick != null)
            {
                _view.PlaySoundEffect(SoundEffects.Click);
                _onClick.Execute();
                return true;
            }

            if (_onClickAction != null)
            {
                _view.PlaySoundEffect(SoundEffects.Click);
                _onClickAction.Execute();
                return true;
            }
            return false;
        }

        void SetClickable(object value)
        {
            _clickable = value != null;
            if (View != null)
                View.Clickable = value != null;
        }


        static bool EditableExist(IContainer continer)
        {
            foreach (var item in continer.Controls)
            {
                if (item is EditText || item is MemoEdit)
                    return true;

                var subc = item as IContainer;
                if (subc != null && EditableExist(subc))
                    return true;
            }

            return false;
        }
    }
}
