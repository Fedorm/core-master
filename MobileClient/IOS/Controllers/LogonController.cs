using System;
using System.Drawing;
using BitMobile.Application.Translator;
using BitMobile.Common.Application;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
	public delegate void LogonCompletedDelegate (bool clearCache);

    public class LogonController : StartScreenController
    {
        private readonly bool _clearCache;
        private string _ctorText;
        private bool _disposed;
        private UIButton _logon;
        private LogonCompletedDelegate _logonComplete;
        private UITextView _message;
        private UITextField _password;
        private UITextField _userName;

        public LogonController(bool clearCache, IApplicationSettings settings, LogonCompletedDelegate logonComplete,
            String message = null)
            : base(settings)
        {
            _clearCache = clearCache;
            _logonComplete = logonComplete;
            _ctorText = message;
        }

        protected override void LayoutContent(UIView container, ContentSet content, LayoutSet layout)
        {
            _message = new UITextView();
            _message.Frame = new RectangleF(layout.margin, layout.offset + 2, layout.contentWidth, 40);
            _message.Editable = false;
            _message.Text = _ctorText ?? D.TO_GET_STARTED_YOU_HAVE_TO_LOGIN;
            _message.TextColor = _ctorText != null ? RED : GRAY;
            _message.BackgroundColor = UIColor.Clear;
            _message.TextAlignment = UITextAlignment.Center;
            _message.Font = UIFont.FromName("Arial", 12);
            container.AddSubview(_message);

            _userName = new UITextField();
            _userName.Frame = new RectangleF(layout.margin, _message.Frame.Bottom + 2, layout.contentWidth, 44);
            _userName.Placeholder = D.USER_NAME;
            _userName.AutocorrectionType = UITextAutocorrectionType.No;
            _userName.ClipsToBounds = true;
            _userName.Layer.BorderColor = content.borderColor.CGColor;
            _userName.Layer.BorderWidth = 1;
            _userName.Layer.CornerRadius = layout.cornerRadius;
            _userName.LeftViewMode = UITextFieldViewMode.Always;
            var userNameImage = new UIImageView();
            userNameImage.Image = content.usernameImg;
            float tfUserNameImageHeight = _userName.Frame.Height/2;
            float tfUserNameImageWidth = tfUserNameImageHeight*userNameImage.Image.Size.Width/
                                         userNameImage.Image.Size.Height;
            userNameImage.Frame = new RectangleF(0, 0, tfUserNameImageWidth + 20, tfUserNameImageHeight);
            userNameImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            _userName.LeftView = userNameImage;
            _userName.Text = _settings.UserName;
            container.AddSubview(_userName);

            _password = new UITextField();
            _password.Frame = new RectangleF(layout.margin, _userName.Frame.Bottom + 10, layout.contentWidth, 44);
            _password.Placeholder = D.PASSWORD;
            _password.SecureTextEntry = true;
            _password.ClipsToBounds = true;
            _password.Layer.BorderColor = content.borderColor.CGColor;
            _password.Layer.BorderWidth = 1;
            _password.Layer.CornerRadius = layout.cornerRadius;
            _password.LeftViewMode = UITextFieldViewMode.Always;
            var passwordImage = new UIImageView();
            passwordImage.Image = content.passwordImg;
            float tfPasswordImageHeight = _password.Frame.Height/2;
            float tfPasswordImageWidth = tfPasswordImageHeight*passwordImage.Image.Size.Width/
                                         passwordImage.Image.Size.Height;
            passwordImage.Frame = new RectangleF(0, 0, tfPasswordImageWidth + 20, tfPasswordImageHeight);
            passwordImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            _password.LeftView = passwordImage;
            if (_settings.DevelopModeEnabled)
                _password.Text = _settings.Password;
            container.AddSubview(_password);

            _logon = new UIButton(UIButtonType.System);
            _logon.Frame = new RectangleF(layout.margin, _password.Frame.Bottom + 10, layout.contentWidth, 44);
            _logon.SetTitle(D.LOGON, UIControlState.Normal);
            _logon.BackgroundColor = content.buttonColor;
            _logon.SetTitleColor(UIColor.White, UIControlState.Normal);
            _logon.Layer.BorderWidth = 0;
            _logon.Layer.CornerRadius = layout.cornerRadius;
            _logon.TouchUpInside += Logon;
            container.AddSubview(_logon);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            View.EndEditing(true);
        }

        protected override float GetPositionToMove()
        {
            return _logon.Frame.Bottom;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                _settings = null;
                _logonComplete = null;
                if (_message != null)
                    _message.Dispose();
                _message = null;
                if (_userName != null)
                    _userName.Dispose();
                _userName = null;
                if (_password != null)
                    _password.Dispose();
                _password = null;
                if (_logon != null)
                    _logon.Dispose();
                _logon = null;

                _disposed = true;
            }
        }

        public void UpdateMessage(String message)
        {
            if (message != null)
            {
                if (_message != null)
                {
                    _message.Text = message;
                    _message.TextColor = RED;
                }
                else
                    _ctorText = message;
            }
        }

        private void Logon(object sender, EventArgs e)
        {
            View.EndEditing(true);

            String userName = _userName.Text;
            String userPassword = _password.Text;
            if (!(String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(userPassword)))
            {
                _settings.UserName = userName;
                _settings.Password = userPassword;
                _settings.WriteSettings();

                if (_logonComplete != null)
					_logonComplete (_clearCache);
            }
        }
    }
}