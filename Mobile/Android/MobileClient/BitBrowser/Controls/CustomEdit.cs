using Android.Text;
using Android.Views;
using BitMobile.Application;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using BitMobile.Utilities.Translator;
using System.Text.RegularExpressions;

namespace BitMobile.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public class CustomEdit : Control<CustomEdit.EditTextNative>, IDataBind, IFocusable, IValidatable, IApplicationContextAware
    {
        const string KeyboardAuto = "auto";
        const string KeyboardDefault = "default";
        const string KeyboardNumeric = "numeric";
        const string KeyboardEmail = "email";
        const string KeyboardUrl = "url";

        private IApplicationContext _applicationContext;
        private string _text;
        private string _placeholder;

        protected CustomEdit(BaseScreen activity)
            : base(activity)
        {
            Keyboard = KeyboardAuto;
            Length = 0;
            Required = false;
            Mask = string.Empty;
        }

        public string Text
        {
            get
            {
                return _text ?? string.Empty;
            }
            set
            {
                _text = value;
                if (_view != null)
                {
                    int positionFromBack = _view.Text.Length - _view.SelectionStart;
                    positionFromBack = positionFromBack > 0 ? positionFromBack : 0;
                    _view.Text = value;
                    int newIndex = _view.Text.Length - positionFromBack;
                    if (newIndex >= 0 && newIndex <= value.Length)
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

        public ActionHandlerEx OnChange { get; set; }

        public ActionHandlerEx OnGetFocus { get; set; }

        public ActionHandlerEx OnLostFocus { get; set; }

        public int Length { get; set; }

        public bool Required { get; set; }

        public string Mask { get; set; }

        public void SetFocus()
        {
            if (_view != null)
            {
                _applicationContext.InvokeOnMainThread(() =>
                    {
                        _view.RequestFocus();
                        _activity.ShowSoftInput(_view);
                    });
            }
            else
                AutoFocus = true;
        }

        public override View CreateView()
        {
            _view = new EditTextNative(_activity);
            _view.SetSelectAllOnFocus(true);
            _view.Text = _text;
            _view.Gravity = GravityFlags.Top;
            _view.FocusChange += View_FocusChange;
            _view.TextChanged += EditText_TextChanged;
            _view.Hint = _placeholder;

            switch (Keyboard.ToLower())
            {
                case KeyboardAuto:
                    if (Value != null && Value.IsNumeric())
                        _view.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
                    else
                        _view.InputType = InputTypes.ClassText;
                    break;
                case KeyboardDefault:
                    _view.InputType = InputTypes.ClassText;
                    break;
                case KeyboardNumeric:
                    _view.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagSigned | InputTypes.NumberFlagDecimal;
                    break;
                case KeyboardEmail:
                    _view.InputType = InputTypes.ClassText | InputTypes.TextVariationEmailAddress;
                    break;
                case KeyboardUrl:
                    _view.InputType = InputTypes.ClassText | InputTypes.TextVariationUri;
                    break;
                default:
                    _view.InputType = InputTypes.ClassText;
                    break;
            }

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // background color, borders
            var background = style.Background(this, _applicationContext, true);
            _view.SetBackgroundDrawable(background);

            // text color
            _view.SetTextColor(style.ColorOrTransparent<Color>(this));

            // placeholder color
            var hintColor = style.Color<PlaceholderColor>(this);
            if (hintColor != null)
            {
                string text = _view.Text;
                _view.Text = null;
                _view.SetHintTextColor(hintColor.Value);
                _view.Text = text;
            }

            // font
            style.SetFontSettings(this, _view, styleBound.Height);

            // padding
            int pl = style.Padding<PaddingLeft>(this, styleBound.Width).Round();
            int pt = style.Padding<PaddingTop>(this, styleBound.Height).Round();
            int pr = style.Padding<PaddingRight>(this, styleBound.Width).Round();
            int pb = style.Padding<PaddingBottom>(this, styleBound.Height).Round();
            View.SetPadding(pl, pt, pr, pb);

            // text align
            switch (style.TextAlign(this))
            {
                case TextAlign.Align.Left:
                    _view.Gravity = GravityFlags.Left;
                    break;
                case TextAlign.Align.Center:
                    _view.Gravity = GravityFlags.Center;
                    break;
                case TextAlign.Align.Right:
                    _view.Gravity = GravityFlags.Right;
                    break;
            }
            return styleBound;
        }

        #region IDataBind

        [DataBindAttribute("Text")]
        public DataBinder Value { get; set; }

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

            string text = _view != null ? _view.Text : _text;

            if (Length > 0 && text.Length > Length)
                msg = D.TEXT_TOO_LONG;
            else if (Required && string.IsNullOrWhiteSpace(text))
                msg = D.FIELD_SHOULDNT_BE_EMPTY;
            else if (!Regex.IsMatch(text, Mask))
                msg = D.INVALID_VALUES;
            else
                result = true;

            if (!result && _view != null)
                _view.Error = msg;

            return result;
        }
        #endregion

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (Droid.ApplicationContext)applicationContext;
        }
        #endregion

        void View_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                _activity.ShowSoftInput(View);
                if (OnGetFocus != null)
                    OnGetFocus.Execute();
            }
            else if (OnLostFocus != null)
                OnLostFocus.Execute();
        }

        void EditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_view.IsShown)
            {
                _text = _view.Text;

                if (Value != null)
                    Value.ControlChanged(e.Text.ToString());

                if (OnChange != null)
                    OnChange.Execute();
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
