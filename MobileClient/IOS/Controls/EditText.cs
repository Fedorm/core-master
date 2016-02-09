using System;
using System.Collections.Generic;
using System.Drawing;
using BitMobile.Application;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "EditText")]
    public class EditText : CustomEdit<EditText.NativeTextField>, IDataBind, IFocusable, IApplicationContextAware,
        IValidatable
    {
        private bool _enabled = true;
        private string _placeholder;
        private UIColor _placeholderColor;
        private string _text;

        public EditText()
        {
            Keyboard = "auto";
            Length = 0;
            Required = false;
            Mask = string.Empty;

            TabOrderManager.Current.Add(this);
        }

        public override string Text
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

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                if (_view != null)
                    SetupPlaceholder(_placeholder);
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_view != null)
                    _view.Enabled = _enabled;
            }
        }

        public IActionHandlerEx OnChange { get; set; }

        public IActionHandlerEx OnGetFocus { get; set; }

        public IActionHandlerEx OnLostFocus { get; set; }

        public void SetFocus()
        {
            if (Enabled)
                if (_view != null)
                {
                    CurrentContext.InvokeOnMainThread(() => _view.BecomeFirstResponder());
                }
                else
                    AutoFocus = true;
        }

        public override void CreateView()
        {
            _view = new NativeTextField();
            _view.Enabled = _enabled;

            _view.TextAlignment = UITextAlignment.Left;
            _view.EditingChanged += HandleEditingChanged;
            _view.TouchUpOutside += HandleTouchUpOutside;
            _view.EditingDidBegin += HandleEditingDidBegin;
            _view.Ended += HandleEditingEnded;

            switch (Keyboard.ToLower())
            {
                case "auto":
                    if (Value != null && Value.IsNumeric())
                    {
                        if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                            _view.KeyboardType = UIKeyboardType.DecimalPad;
                        else
                            _view.KeyboardType = UIKeyboardType.NumberPad;
                        _inputValidator.IsNumeric = true;
                    }
                    else
                        _view.KeyboardType = UIKeyboardType.Default;
                    break;
                case "default":
                    _view.KeyboardType = UIKeyboardType.Default;
                    break;
                case "numeric":
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                        _view.KeyboardType = UIKeyboardType.DecimalPad;
                    else
                        _view.KeyboardType = UIKeyboardType.NumberPad;
                    _inputValidator.IsNumeric = true;
                    break;
                case "email":
                    _view.KeyboardType = UIKeyboardType.EmailAddress;
                    break;
                case "url":
                    _view.KeyboardType = UIKeyboardType.Url;
                    break;
                case "phone":
                    _view.KeyboardType = UIKeyboardType.PhonePad;
                    break;
                default:
                    _view.KeyboardType = UIKeyboardType.Default;
                    break;
            }

            if (AutoFocus)
                _view.BecomeFirstResponder();

            TabOrderManager.Current.AttachAccessory(this);
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            IStyleSheetHelper style = stylesheet.Helper;

            base.Apply(stylesheet, styleBound, maxBound);

            // background color, borders
            stylesheet.SetBackgroundSettings(this);

            // text color
            _view.TextColor = style.Color(this).ToColorOrClear();

            // placeholder color
            _placeholderColor = style.PlaceholderColor(this).ToNullableColor();
            SetupPlaceholder(_placeholder);

            // font
            UIFont f = stylesheet.Font(this, styleBound.Height);
            if (f != null)
                _view.Font = f;

            // padding
            _view.PaddingLeft = style.PaddingLeft(this, styleBound.Width);
            _view.PaddingTop = style.PaddingTop(this, styleBound.Height);
            _view.PaddingRight = style.PaddingRight(this, styleBound.Width);
            _view.PaddingBottom = style.PaddingBottom(this, styleBound.Height);

            // text align
            switch (style.TextAlign(this))
            {
                case TextAlignValues.Left:
                    _view.TextAlignment = UITextAlignment.Left;
                    break;
                case TextAlignValues.Center:
                    _view.TextAlignment = UITextAlignment.Center;
                    break;
                case TextAlignValues.Right:
                    _view.TextAlignment = UITextAlignment.Right;
                    break;
            }

            _view.RightView = CreateValidationIcon(_view.Font.LineHeight);

            _view.Text = _text;

            // size to content by background
            return styleBound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, borders
                helper.SetBackgroundSettings(this);

                // text color
                ITextColor textColor;
                if (helper.TryGet(out textColor))
                    _view.TextColor = textColor.ToColorOrClear();

                // placeholder color
                IPlaceholderColor placeholderColor;
                if (helper.TryGet(out placeholderColor))
                {
                    _placeholderColor = placeholderColor.ToNullableColor();
                    SetupPlaceholder(_placeholder);
                }

                // font
                UIFont f;
                if (helper.FontChanged(styleBound.Height, out f))
                    _view.Font = f;

                // padding
                float screenWidth = ApplicationContext.Current.DisplayProvider.Width;
                float screenHeight = ApplicationContext.Current.DisplayProvider.Height;
                _view.PaddingLeft = helper.Get<IPaddingLeft>().CalcSize(styleBound.Width, screenWidth);
                _view.PaddingTop = helper.Get<IPaddingTop>().CalcSize(styleBound.Height, screenHeight);
                _view.PaddingRight = helper.Get<IPaddingRight>().CalcSize(styleBound.Width, screenWidth);
                _view.PaddingBottom = helper.Get<IPaddingBottom>().CalcSize(styleBound.Height, screenHeight);

                // text align
                ITextAlign textAlign;
                if (helper.TryGet(out textAlign))
                    switch (textAlign.Align)
                    {
                        case TextAlignValues.Left:
                            _view.TextAlignment = UITextAlignment.Left;
                            break;
                        case TextAlignValues.Center:
                            _view.TextAlignment = UITextAlignment.Center;
                            break;
                        case TextAlignValues.Right:
                            _view.TextAlignment = UITextAlignment.Right;
                            break;
                    }

                DisposeField(ref _placeholderColor);

                DisposeField(ref _placeholderColor);

                // for padding :)
                _view.RightView = CreateValidationIcon(_view.Font.LineHeight);
            }

            return styleBound;
        }

        protected override void OnValidate(bool succes, string message)
        {
            _view.RightViewMode = succes ? UITextFieldViewMode.Never : UITextFieldViewMode.Always;
        }

        protected override void Dismiss()
        {
            DisposeField(ref _placeholderColor);

            _view.EditingChanged -= HandleEditingChanged;
            _view.TouchUpOutside -= HandleTouchUpOutside;
            _view.EditingDidBegin -= HandleEditingDidBegin;
            _view.Ended -= HandleEditingChanged;

            base.Dismiss();
        }

        private void HandleEditingChanged(object sender, EventArgs e)
        {
            if (_view != null)
            {
                // sometimes unbind events ont working

                HandleOverlayVisiblity(false, true);
                _view.RightViewMode = UITextFieldViewMode.Never;

                OnEditingChanged();
            }
        }

        private void HandleEditingEnded(object sender, EventArgs e)
        {
            if (_view != null)
            {
                HandleOverlayVisiblity(false);

                OnEditingChanged();

                LogManager.Logger.TextInput(Id, Text);

                if (OnLostFocus != null)
                    OnLostFocus.Execute();
            }
        }

        private void HandleTouchUpOutside(object sender, EventArgs e)
        {
            if (_view != null)
                _view.ResignFirstResponder();
        }

        private void HandleEditingDidBegin(object sender, EventArgs e)
        {
            if (_view != null)
            {
                CloseModalWindows();

                HandleOverlayVisiblity(true);

                if (OnGetFocus != null)
                    OnGetFocus.Execute();

                if (_view != null)
                {
                    _view.SelectAll(new NSObject());
                    _view.Selected = true;
                }
            }
        }

        private void OnEditingChanged()
        {
            string old = _text;
            _text = Text;

            if (Value != null && !IOSApplicationContext.Busy)
                Value.ControlChanged(Text);

            _inputValidator.OnChange(Text, old);

            if (OnChange != null)
                OnChange.Execute();
        }

        private void SetupPlaceholder(string text)
        {
            text = text ?? string.Empty;
            if (_placeholderColor != null)
                _view.AttributedPlaceholder = new NSAttributedString(text, null, _placeholderColor);
            else
            {
                _view.AttributedPlaceholder = new NSAttributedString();
                _view.Placeholder = text;
            }
        }

        public class NativeTextField : UITextField
        {
            public float PaddingTop { get; set; }

            public float PaddingLeft { get; set; }

            public float PaddingBottom { get; set; }

            public float PaddingRight { get; set; }

            public override RectangleF TextRect(RectangleF forBounds)
            {
                var newRect = new RectangleF(forBounds.X + PaddingLeft, forBounds.Y + PaddingTop,
                    forBounds.Width - PaddingLeft - PaddingRight, forBounds.Height - PaddingTop - PaddingBottom);
                return base.TextRect(newRect);
            }

            public override RectangleF EditingRect(RectangleF forBounds)
            {
                var newRect = new RectangleF(forBounds.X + PaddingLeft, forBounds.Y + PaddingTop,
                    forBounds.Width - PaddingLeft - PaddingRight, forBounds.Height - PaddingTop - PaddingBottom);
                return base.EditingRect(newRect);
            }
        }
    }
}