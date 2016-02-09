using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using BitMobile.Application.Log;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Button")]
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global
    internal class Button : CustomText<Button.ButtonNative>
    {
        private string _onEvent;

        public Button(BaseScreen activity)
            : base(activity)
        {
            DefaultAlignValues = TextAlignValues.Center;
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public IActionHandler OnClickAction { get; set; }

        public IActionHandlerEx OnClick { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

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

        public string SubmitScope { get; set; }

        public override void CreateView()
        {
            _view = new ButtonNative(Activity)
            {
                Clickable = true,
                Text = Text,
                Gravity = GravityFlags.Center
            };
            _view.TouchEvent += View_TouchInvoke;
            _view.SetIncludeFontPadding(false);
        }

        protected override void ApplyTextAlign(TextAlignValues textAlign)
        {
            switch (textAlign)
            {
                case TextAlignValues.Left:
                    _view.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
                    break;
                case TextAlignValues.Center:
                    _view.Gravity = GravityFlags.Center;
                    break;
                case TextAlignValues.Right:
                    _view.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
                    break;
            }
        }

        protected bool InvokeClickAction()
        {
            if (OnClick != null || OnClickAction != null)
            {
                bool allowed = true;
                if (!string.IsNullOrWhiteSpace(SubmitScope))
                    allowed = CurrentContext.Validate(SubmitScope);

                if (allowed)
                {
                    if (OnClick != null)
                    {
                        LogManager.Logger.Clicked(Id, OnClick.Expression, Text);
                        CurrentContext.JokeProviderInternal.OnTap();
                        _view.PlaySoundEffect(SoundEffects.Click);
                        OnClick.Execute();
                        return true;
                    }

                    if (OnClickAction != null)
                    {
                        LogManager.Logger.Clicked(Id, OnClickAction.Expression, Text);
                        CurrentContext.JokeProviderInternal.OnTap();
                        _view.PlaySoundEffect(SoundEffects.Click);
                        OnClickAction.Execute();
                        return true;
                    }
                }
            }

            return false;
        }

        private void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Up)
                if (!CurrentContext.CurrentNativeScreen.GestureHolded())
                    InvokeClickAction();

            if (OnClick != null || OnClickAction != null)
                DoTouchAnimation(e.Event);

            e.Handled = true;
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
