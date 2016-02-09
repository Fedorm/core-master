using System.Drawing;
using System.Text.RegularExpressions;
using BitMobile.Application.Controls;
using BitMobile.Application.Translator;
using BitMobile.Common.Controls;
using BitMobile.Controls;
using BitMobile.UI;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public abstract class CustomEdit<T> : Control<T>, IDataBind, IFocusable, IValidatable
        where T : UIView
    {
        private const string ValidationErrorIcon = "warning.png";

        private UILabel _errorOverlay;
        protected IInputValidator _inputValidator;        
        private bool _validationError;

        public CustomEdit()
        {
            _inputValidator = ControlsContext.Current.CreateInputValidator(this, "Text");
        }

        public abstract string Text { get; set; }

        public int Length { get; set; }

        public bool Required { get; set; }

        public string Mask { get; set; }

        #region IDataBind implementation

        [DataBind("Text")]
        public IDataBinder Value { get; set; }

        public void DataBind()
        {
        }

        #endregion

        #region IFocusable implementation

        public bool AutoFocus { get; set; }

        public string Keyboard { get; set; }

        #endregion

        #region IValidatable implementation

        public bool Validate()
        {
            bool result = false;
            string msg = string.Empty;

			string text = Text ?? string.Empty; // iOS9 NullReference fix

            if (Length > 0 && text.Length > Length)
                msg = D.TEXT_TOO_LONG;
            else if (Required && string.IsNullOrEmpty(text))
                msg = D.FIELD_SHOULDNT_BE_EMPTY;
            else if (Mask != null && !Regex.IsMatch(text, Mask))
                msg = D.INVALID_VALUES;
            else
                result = true;

            _validationError = !result;

            if (_view != null)
            {
                SetupValidation(msg);
                OnValidate(result, msg);
            }

            return result;
        }

        #endregion

        protected abstract void OnValidate(bool success, string message);

        protected override void Dismiss()
        {
            DisposeField(ref _errorOverlay);            
        }

        protected void HandleOverlayVisiblity(bool visible, bool disable = false)
        {
            if (_validationError && _errorOverlay != null)
            {
                _errorOverlay.Hidden = !visible;
                if (disable)
                    _validationError = false;
            }
        }

        protected UIImageView CreateValidationIcon(float size)
        {
            var img = new UIImageView(new RectangleF(0, 0, size, size));
            img.Image = new UIImage(ValidationErrorIcon);
            img.ContentMode = UIViewContentMode.ScaleAspectFit;
            return img;
        }

        private void SetupValidation(string msg)
        {
            if (_view != null)
            {
                if (_errorOverlay != null)
                    _errorOverlay.Dispose();
                _errorOverlay = CreateErrorOverlay(msg);
            }
        }

        private UILabel CreateErrorOverlay(string message)
        {
            var overlay = new UILabel();
            overlay.Hidden = true;
            overlay.BackgroundColor = UIColor.White;
            overlay.TextAlignment = UITextAlignment.Center;
            overlay.Layer.BorderColor = UIColor.Red.CGColor;
            overlay.Layer.BorderWidth = 1;
            overlay.Layer.CornerRadius = 2;
            overlay.Text = message;
            overlay.Font = UIFont.SystemFontOfSize(UIFont.SmallSystemFontSize);

            SizeF baseSize = overlay.SizeThatFits(UIScreen.MainScreen.ApplicationFrame.Size);
            var size = new SizeF(baseSize.Width + 16, baseSize.Height + 8);

            LayoutOverlay(overlay, size, _view.Superview, _view.Frame.Location);

            return overlay;
        }
    }
}