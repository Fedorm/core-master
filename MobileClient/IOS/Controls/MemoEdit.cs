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
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "MemoEdit")]
    public class MemoEdit : CustomEdit<UITextView>, IDataBind, IFocusable, IApplicationContextAware, IValidatable
    {
        private bool _enabled = true;
        private UIImageView _errorSubview;
        private string _placeholder;
        private UIColor _placeholderColor;
        private bool _placeholderVisible;
        private string _text = "";
        private UIColor _textColor;

        public MemoEdit()
        {
            Keyboard = "auto";
            Length = 0;
            Required = false;
            Mask = string.Empty;

            TabOrderManager.Current.Add(this);
        }

        public override string Text
        {
            get { return _text ?? ""; }
            set
            {
                _text = value;
                if (_view != null)
                {
                    _view.Text = _text;
                    RefreshPlaceholder();
                }
            }
        }

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                RefreshPlaceholder();
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_view != null)
                {
                    _view.Editable = _enabled;
                    _view.Selectable = _enabled;
                }
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
            _view = new UITextView();
            _view.Text = _text;
            _view.Editable = _enabled;
            _view.Selectable = _enabled;
            _view.Changed += HandleChanged;
            _view.Ended += HandleChanged;
            _view.Ended += HandleEnded;
            _view.Started += HandleStarted;
            _view.SelectionChanged += HandleSelectionChanged;
            if (IOSApplicationContext.OSVersion.Major >= 7)
                _view.TextContainer.LineFragmentPadding = 0;

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
            base.Apply(stylesheet, styleBound, maxBound);

            IStyleSheetHelper style = stylesheet.Helper;

            // background color, borders
            stylesheet.SetBackgroundSettings(this);

            // text color
            _textColor = style.Color(this).ToColorOrClear();
            _view.TextColor = _textColor;

            // placeholder color
            _placeholderColor = style.PlaceholderColor(this).ToColorOrClear();

            // font
            UIFont f = stylesheet.Font(this, styleBound.Height);
            if (f != null)
                _view.Font = f;

            // padding
            float pl = style.PaddingLeft(this, styleBound.Width);
            float pt = style.PaddingTop(this, styleBound.Height);
            float pr = style.PaddingRight(this, styleBound.Width);
            float pb = style.PaddingBottom(this, styleBound.Height);
            if (IOSApplicationContext.OSVersion.Major >= 7)
                _view.TextContainerInset = new UIEdgeInsets(pt, pl, pb, pr);

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

            RefreshPlaceholder();

            float size = _view.Font.LineHeight;
            _errorSubview = CreateValidationIcon(size);
            _errorSubview.Frame = new RectangleF(styleBound.Width - size, pt, size, size);
            _errorSubview.Hidden = true;
            _view.AddSubview(_errorSubview);

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
                {
                    _textColor = textColor.ToColorOrClear();
                    _view.TextColor = _textColor;
                }

                // placeholder color
                IPlaceholderColor placeholderColor;
                if (helper.TryGet(out placeholderColor))
                {
                    _placeholderColor = placeholderColor.ToColorOrClear();
                    if (!string.IsNullOrEmpty(_placeholder) && _placeholderVisible)
                        _view.TextColor = _placeholderColor;
                }

                // font
                UIFont f;
                if (helper.FontChanged(styleBound.Height, out f))
                    _view.Font = f;

                // padding
                float screenWidth = ApplicationContext.Current.DisplayProvider.Width;
                float screenHeight = ApplicationContext.Current.DisplayProvider.Height;
                float pl = helper.Get<IPaddingLeft>().CalcSize(styleBound.Width, screenWidth);
                float pt = helper.Get<IPaddingTop>().CalcSize(styleBound.Height, screenHeight);
                float pr = helper.Get<IPaddingRight>().CalcSize(styleBound.Width, screenWidth);
                float pb = helper.Get<IPaddingBottom>().CalcSize(styleBound.Height, screenHeight);
                if (IOSApplicationContext.OSVersion.Major >= 7)
                    _view.TextContainerInset = new UIEdgeInsets(pt, pl, pb, pr);

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

                float size = _view.Font.LineHeight;
                _errorSubview.Frame = new RectangleF(styleBound.Width - size, pt, size, size);
                _errorSubview.Hidden = true;
            }

            return styleBound;
        }

        protected override void OnValidate(bool succes, string message)
        {
            _errorSubview.Hidden = succes;
        }

        protected override void Dismiss()
        {
            DisposeField(ref _errorSubview);
            DisposeField(ref _placeholderColor);
            DisposeField(ref _textColor);

            _view.Changed -= HandleChanged;
            _view.Ended -= HandleChanged;
            _view.Ended -= HandleEnded;
            _view.Started -= HandleStarted;

            base.Dismiss();
        }

        private void HandleChanged(object sender, EventArgs e)
        {
            if (_view != null)
            {
                // sometimes unbind events ont working
                RefreshPlaceholder();

                HandleOverlayVisiblity(false, true);
                _errorSubview.Hidden = true;

                string old = _text;
                _text = _view.Text;

                if (Value != null && !IOSApplicationContext.Busy)
                    Value.ControlChanged(Text);

                if (!_placeholderVisible)
                    _inputValidator.OnChange(_text, old);

                if (OnChange != null)
                {
                    OnChange.Execute();
                }
            }
        }

        private void HandleEnded(object sender, EventArgs e)
        {
            if (_view != null)
            {
                _view.ResignFirstResponder();

                HandleOverlayVisiblity(false);

                LogManager.Logger.TextInput(Id, Text);

                if (OnLostFocus != null)
                    OnLostFocus.Execute();
            }
        }

        private void HandleStarted(object sender, EventArgs e)
        {
            if (_view != null)
            {
                CloseModalWindows();

                HandleOverlayVisiblity(true);

                if (OnGetFocus != null)
                    OnGetFocus.Execute();

                if (!_placeholderVisible)
                    _view.SelectAll(new NSObject());
                else
                    _view.SelectedRange = new NSRange(0, 0);
            }
        }

        private void HandleSelectionChanged(object sender, EventArgs e)
        {
            if (_view != null && _placeholderVisible)
                _view.SelectedRange = new NSRange(0, 0);
        }

        private void RefreshPlaceholder()
        {
            if (_view != null && !string.IsNullOrEmpty(_placeholder))
            {
                if (_placeholderVisible)
                {
                    if (_view.Text != _placeholder)
                    {
                        _placeholderVisible = false;
                        _view.TextColor = _textColor;
                        _view.Text = _view.Text.Replace(_placeholder, "");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(_view.Text))
                    {
                        _placeholderVisible = true;
                        _view.TextColor = _placeholderColor;
                        _view.Text = _placeholder;
                    }
                }
            }
        }
    }
}