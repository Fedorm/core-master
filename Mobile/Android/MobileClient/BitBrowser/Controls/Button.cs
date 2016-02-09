using Android.Content;
using Android.Runtime;
using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using System;

namespace BitMobile.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global
    internal class Button : Control<Button.ButtonNative>, IApplicationContextAware, IImageContainer
    {
        // ReSharper disable once InconsistentNaming
        protected ApplicationContext _applicationContext;

        private ActionHandler _onClickAction;
        private ActionHandlerEx _onClick;
        private Android.Graphics.Color _textColor;
        private Android.Graphics.Color? _selectedColor;
        private bool _clickable;
        private string _text = "";

        public Button(BaseScreen activity)
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

        public string Text
        {
            get
            {
                if (_view != null)
                    return _view.Text;
                return _text;
            }
            set
            {
                if (_view != null)
                    _view.Text = value;
                _text = value;
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
            _view = new ButtonNative(_activity)
            {
                Clickable = true,
                Text = _text,
                Gravity = GravityFlags.Center
            };
            _view.TouchEvent += View_TouchInvoke;
            _view.SetIncludeFontPadding(false);
            _view.SetBackgroundColor(Android.Graphics.Color.Transparent);
            _view.SetPadding(0, 0, 0, 0);
            _view.SetSingleLine();
            _view.Ellipsize = Android.Text.TextUtils.TruncateAt.Middle;
            _view.Clickable = _clickable;

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // background color, background image, borders
            var background = style.Background(this, _applicationContext);
            _view.SetBackgroundDrawable(background);

            // font
            style.SetFontSettings(this, _view, styleBound.Height);

            // text color
            _textColor = style.ColorOrTransparent<Color>(this);
            _view.SetTextColor(_textColor);

            //selected color
            _selectedColor = style.Color<SelectedColor>(this);

            return styleBound;
        }

        public override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    AnimationPress();
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    AnimationRelease();
                    break;
            }
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (ApplicationContext)applicationContext;
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

        protected virtual bool InvokeClickAction()
        {
            if (OnClick != null)
            {
                _view.PlaySoundEffect(SoundEffects.Click);
                OnClick.Execute();
                return true;
            }

            if (OnClickAction != null)
            {
                _view.PlaySoundEffect(SoundEffects.Click);
                OnClickAction.Execute();
                return true;
            }
            return false;
        }

        void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Up)
                if (!_applicationContext.CurrentNativeScreen.GestureHolded())
                    InvokeClickAction();

            if (OnClick != null || OnClickAction != null)
                AnimateTouch(e.Event);

            e.Handled = true;
        }

        void AnimationPress()
        {
            if (_selectedColor != null)
                _view.SetTextColor(_selectedColor.Value);
            _view.Invalidate();
        }

        void AnimationRelease()
        {
            _view.SetTextColor(_textColor);
            _view.Invalidate();
        }

        void SetClickable(object value)
        {
            _clickable = value != null;
            if (View != null)
                View.Clickable = value != null;
        }

        public class ButtonNative : Android.Widget.Button
        {
            public event EventHandler<TouchEventArgs> TouchEvent;

            public ButtonNative(Context activity)
                : base(activity)
            {
            }

            public ButtonNative(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            public override bool DispatchTouchEvent(MotionEvent e)
            {
                base.DispatchTouchEvent(e);
                TouchEvent.Execute(this, e);
                return true;
            }
        }
    }
}
