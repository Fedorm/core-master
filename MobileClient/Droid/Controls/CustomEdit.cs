using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Application.Translator;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Controls;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;
using InputTypes = Android.Text.InputTypes;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "CustomEdit")]
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public abstract class CustomEdit : Control<CustomEdit.EditTextNative>, IDataBind, IFocusable, IValidatable
    {
        const string KeyboardAuto = "auto";
        const string KeyboardDefault = "default";
        const string KeyboardNumeric = "numeric";
        const string KeyboardEmail = "email";
        const string KeyboardUrl = "url";
        const string KeyboardPhone = "phone";

        private readonly BaseScreen _activity;
        private readonly IInputValidator _inputValidator;
        private string _text;
        private string _placeholder;
        private bool _warningVisible;
        private bool _enabled = true;

        protected CustomEdit(BaseScreen activity)
            : base(activity)
        {
            _activity = activity;
            Keyboard = KeyboardAuto;
            Length = 0;
            Required = false;
            Mask = string.Empty;
            _inputValidator = ControlsContext.Current.CreateInputValidator(this, "Text");
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? string.Empty;
                if (_view != null)
                {
                    int positionFromBack = _view.Text.Length - _view.SelectionStart;
                    positionFromBack = positionFromBack > 0 ? positionFromBack : 0;
                    _view.Text = value ?? "";
                    int newIndex = _view.Text.Length - positionFromBack;
                    if (newIndex >= 0 && newIndex <= _text.Length)
                        _view.SetSelection(newIndex);
                }
            }
        }

        public string Placeholder
        {
            get
            {
                if (_view != null)
                    return _view.Hint;
                return _placeholder;
            }
            set
            {
                _placeholder = value;
                if (_view != null)
                    _view.Hint = _placeholder;
            }
        }


        public bool Enabled
        {
            get
            {
                return _enabled;
            }
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

        public int Length { get; set; }

        public bool Required { get; set; }

        public string Mask { get; set; }

        public void SetFocus()
        {
            if (Enabled)
                if (_view != null)
                {
                    CurrentContext.InvokeOnMainThread(() =>
                    {
                        _view.RequestFocus();
                        _activity.ShowSoftInput(_view);
                    });
                }
                else
                    AutoFocus = true;
        }

        public override void CreateView()
        {
            _view = new EditTextNative(Activity);
            _view.SetSelectAllOnFocus(true);
            _view.Text = _text;
            _view.Gravity = GravityFlags.Top;
            _view.FocusChange += View_FocusChange;
            _view.TextChanged += EditText_TextChanged;
            _view.Hint = _placeholder;
            _view.Enabled = _enabled;

            InputTypes inputType;
            switch (Keyboard.ToLower())
            {
                case KeyboardAuto:
                    if (Value != null && Value.IsNumeric())
                    {
                        inputType = InputTypes.NumberFlagSigned | InputTypes.NumberFlagDecimal;
                        _inputValidator.IsNumeric = true;
                    }
                    else
                        inputType = InputTypes.ClassText;
                    break;
                case KeyboardDefault:
                    inputType = InputTypes.ClassText;
                    break;
                case KeyboardNumeric:
                    inputType = InputTypes.ClassNumber | InputTypes.NumberFlagSigned | InputTypes.NumberFlagDecimal;
                    _inputValidator.IsNumeric = true;
                    break;
                case KeyboardEmail:
                    inputType = InputTypes.ClassText | InputTypes.TextVariationEmailAddress;
                    break;
                case KeyboardUrl:
                    inputType = InputTypes.ClassText | InputTypes.TextVariationUri;
                    break;
                case KeyboardPhone:
                    inputType = InputTypes.ClassPhone;
                    break;
                default:
                    inputType = InputTypes.ClassText;
                    break;
            }

            if (IsMultiline())
                inputType |= InputTypes.TextFlagMultiLine;
            _view.SetRawInputType(inputType);
        }

        #region IDataBind

        [DataBind("Text")]
        public IDataBinder Value { get; set; }

        public void DataBind()
        {
        }
        #endregion

        #region IFocusable

        public bool AutoFocus { get; set; }

        public string Keyboard { get; set; }
        #endregion

        #region IValidatable

        public bool Validate()
        {
            bool result = false;
            string msg = string.Empty;

            string text = (_view != null ? _view.Text : _text) ?? "";

            if (Length > 0 && text.Length > Length)
                msg = D.TEXT_TOO_LONG;
            else if (Required && string.IsNullOrEmpty(text))
                msg = D.FIELD_SHOULDNT_BE_EMPTY;
            else if (Mask != null && !Regex.IsMatch(text, Mask))
                msg = D.INVALID_VALUES;
            else
                result = true;

            if (!result && _view != null)
                SetupError(msg);

            return result;
        }
        #endregion

        protected abstract bool IsMultiline();

        protected sealed override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            IStyleSheetHelper style = stylesheet.Helper;

            // background color, borders
            using (var background = stylesheet.Background(this, styleBound, true))
                SetBackground(background);

            // text color
            Color textColor = style.Color(this).ToColorOrTransparent();
            _view.SetTextColor(textColor);

            SetCursor();

            // placeholder color
            var hintColor = style.PlaceholderColor(this).ToNullableColor();
            if (hintColor != null)
            {
                string text = _view.Text;
                _view.Text = null;
                _view.SetHintTextColor(hintColor.Value);
                _view.Text = text;
            }

            // font
            stylesheet.SetFontSettings(this, _view, styleBound.Height);

            // padding
            int pl = style.PaddingLeft(this, styleBound.Width).Round();
            int pt = style.PaddingTop(this, styleBound.Height).Round();
            int pr = style.PaddingRight(this, styleBound.Width).Round();
            int pb = style.PaddingBottom(this, styleBound.Height).Round();
            View.SetPadding(pl, pt, pr, pb);

            // text align
            switch (style.TextAlign(this))
            {
                case TextAlignValues.Left:
                    _view.Gravity = GravityFlags.Top | GravityFlags.Left;
                    break;
                case TextAlignValues.Center:
                    _view.Gravity = GravityFlags.Top | GravityFlags.CenterHorizontal;
                    break;
                case TextAlignValues.Right:
                    _view.Gravity = GravityFlags.Top | GravityFlags.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                _view.SetSingleLine(IsMultiline()); // fix disappearing text in android 5.0+
            }

            return GetBoundByBackgroud(styleBound, maxBound);
        }

        private void SetCursor()
        {
            try
            {
                //https://forums.xamarin.com/discussion/42823/change-entry-cursor
                IntPtr intPtrtextViewClass = JNIEnv.FindClass(typeof(Android.Widget.TextView));
                IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(intPtrtextViewClass, "mCursorDrawableRes", "I");
                JNIEnv.SetField(_view.Handle, mCursorDrawableResProperty, Resource.Drawable.cursor);
            }
            catch (Exception e)
            {
                CurrentContext.HandleException(new NonFatalException(D.ERROR, "SerCursor error", e));
            }
        }

        protected sealed override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, borders
                Drawable background;
                if (helper.BackgroundChanged(CurrentStyleSheet, Frame.Bound, out background))
                    using (background)
                        SetBackground(background);

                // text color
                ITextColor textColor;
                if (helper.TryGet(out textColor))
                    _view.SetTextColor(textColor.ToColorOrTransparent());


                // placeholder color
                IPlaceholderColor placeholderColor;
                if (helper.TryGet(out placeholderColor))
                {
                    var color = placeholderColor.ToNullableColor();
                    if (color != null)
                    {
                        string text = _view.Text;
                        _view.Text = null;
                        _view.SetHintTextColor(color.Value);
                        _view.Text = text;
                    }
                }

                // font
                helper.SetFontSettings(_view, styleBound.Height);

                // text padding
                int pl = helper.GetSizeOrDefault<IPaddingLeft>(styleBound.Width, _view.PaddingLeft).Round();
                int pt = helper.GetSizeOrDefault<IPaddingTop>(styleBound.Height, _view.PaddingTop).Round();
                int pr = helper.GetSizeOrDefault<IPaddingRight>(styleBound.Width, _view.PaddingRight).Round();
                int pb = helper.GetSizeOrDefault<IPaddingBottom>(styleBound.Height, _view.PaddingBottom).Round();
                _view.SetPadding(pl, pt, pr, pb);

                // text align
                ITextAlign textAlign;
                if (helper.TryGet(out textAlign))
                switch (textAlign.Align)
                    {
                        case TextAlignValues.Left:
                            _view.Gravity = GravityFlags.Left;
                            break;
                        case TextAlignValues.Center:
                            _view.Gravity = GravityFlags.Center;
                            break;
                        case TextAlignValues.Right:
                            _view.Gravity = GravityFlags.Right;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    _view.SetSingleLine(IsMultiline());// fix disappearing text in android 5.0+
                }
            }


            return GetBoundByBackgroud(styleBound, maxBound);
        }

        private void View_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Activity.ShowSoftInput(View);
                if (OnGetFocus != null)
                    OnGetFocus.Execute();
            }
            else
            {
                LogManager.Logger.TextInput(Id, Text);

                if (OnLostFocus != null)
                    OnLostFocus.Execute();
            }
        }

        private void EditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_view.IsShown)
            {
                HideWarning();

                string old = _text;
                _text = _view.Text;

                if (Value != null)
                    Value.ControlChanged(e.Text.ToString());

                _inputValidator.OnChange(_text, old);

                if (OnChange != null)
                    OnChange.Execute();
            }
        }

        private void SetupError(string msg)
        {
            _view.SetError(msg, null);
            if (msg != null)
            {
                int size = _view.LineHeight;// because line height for small letters
                using (var bitmap = BitmapFactory.DecodeResource(BitBrowserApp.Current.BaseActivity.Resources, Resource.Drawable.warning))
                using (var scaled = Bitmap.CreateScaledBitmap(bitmap, size, size, true))
                using (var icon = new BitmapDrawable(scaled))
                    _view.SetCompoundDrawablesRelativeWithIntrinsicBounds(null, null, icon, null);
                _warningVisible = true;
            }
            else
                HideWarning();
        }

        private void HideWarning()
        {
            if (_warningVisible)
            {
                _view.SetError((string)null, null);
                _view.SetError((Java.Lang.ICharSequence)null, null);
                _view.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
                _warningVisible = false;
            }
        }

        public class EditTextNative : Android.Widget.EditText
        {
            public EditTextNative(BaseScreen activity)
                : base(activity)
            {
            }
        }
    }
}
