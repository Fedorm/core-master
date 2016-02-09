using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;
using System.Drawing;
using BitMobile.Controls.StyleSheet;
using System.Text.RegularExpressions;
using BitMobile.Utilities.Translator;
using System.IO;

namespace BitMobile.Controls
{
	public class EditText : Control<EditText.NativeTextField>, IDataBind, IFocusable, IApplicationContextAware, IValidatable
	{
		ApplicationContext _applicationContext = null;
		string _text;
		string _placeholder;
		UIColor _placeholderColor;
		bool _disposed;

		public EditText ()
		{         
			this.Keyboard = "auto";
			this.Length = 0;
			this.Required = false;
			this.Mask = string.Empty;

			TabOrderManager.Current.Add (this);
		}

		public String Text {
			get {
				if (_view != null)
					return _view.Text;
				return _text;
			}
			set {
				if (_view != null)
					_view.Text = value;
				_text = value;
			}
		}

		public String Placeholder {
			get {
				return _placeholder;
			}
			set {
				_placeholder = value;
				if (_view != null) {
					SetupPlaceholder (_placeholder);
				}
			}
		}

		public ActionHandlerEx OnChange { get; set; }

		public ActionHandlerEx OnGetFocus { get; set; }

		public ActionHandlerEx OnLostFocus { get; set; }

		public int Length { get; set; }

		public bool Required { get; set; }

		public string Mask { get; set; }

		public void SetFocus ()
		{
			if (_view != null) {
				_applicationContext.InvokeOnMainThread (() => _view.BecomeFirstResponder ());
			} else
				AutoFocus = true;
		}

		public override UIView CreateView ()
		{
			_view = new NativeTextField ();

			_view.TextAlignment = UITextAlignment.Left;
			_view.EditingChanged += HandleEditingChanged;
			_view.TouchUpOutside += HandleTouchUpOutside;
			_view.EditingDidBegin += HandleEditingDidBegin;
			_view.Ended += HandleEditingEnded;	

			switch (this.Keyboard.ToLower ()) {
			case "auto":
				if (this.Value != null && this.Value.IsNumeric ()) {
					if (UIDevice.CurrentDevice.Model.Contains ("iPhone"))
						_view.KeyboardType = UIKeyboardType.DecimalPad;
					else
						_view.KeyboardType = UIKeyboardType.NumberPad;
				} else
					_view.KeyboardType = UIKeyboardType.Default;
				break;
			case "default":
				_view.KeyboardType = UIKeyboardType.Default;
				break;
			case "numeric":
				if (UIDevice.CurrentDevice.Model.Contains ("iPhone"))
					_view.KeyboardType = UIKeyboardType.DecimalPad;
				else
					_view.KeyboardType = UIKeyboardType.NumberPad;
				break;
			case "email":
				_view.KeyboardType = UIKeyboardType.EmailAddress;
				break;
			case "url":
				_view.KeyboardType = UIKeyboardType.Url;
				break;
			default:
				_view.KeyboardType = UIKeyboardType.Default;
				break;
			}

			if (this.AutoFocus) {
				_view.BecomeFirstResponder ();
			}

			TabOrderManager.Current.AttachAccessory (this);

			return _view;               
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			base.Apply (stylesheet, styleBound, maxBound);

			// background color, borders
			style.SetBackgroundSettings (this);

			// text color
			_view.TextColor = style.ColorOrClear<BitMobile.Controls.StyleSheet.Color> (this);

			// placeholder color
			_placeholderColor = style.Color<PlaceholderColor> (this);
			SetupPlaceholder (_placeholder);

			// font
			UIFont f = style.Font (this, styleBound.Height);
			if (f != null)
				_view.Font = f;

			// padding
			_view.PaddingLeft = style.Padding<PaddingLeft> (this, styleBound.Width);
			_view.PaddingTop = style.Padding<PaddingTop> (this, styleBound.Height);
			_view.PaddingRight = style.Padding<PaddingRight> (this, styleBound.Width);
			_view.PaddingBottom = style.Padding<PaddingBottom> (this, styleBound.Height);

			// text align
			switch (style.TextAlign (this)) {
			case TextAlign.Align.Left:
				_view.TextAlignment = UITextAlignment.Left;
				break;
			case TextAlign.Align.Center:
				_view.TextAlignment = UITextAlignment.Center;
				break;
			case TextAlign.Align.Right:
				_view.TextAlignment = UITextAlignment.Right;
				break;
			}

			_view.Text = _text;

			return styleBound;
		}

		#region IDataBind implementation

		[DataBindAttribute ("Text")]
		public DataBinder Value { get; set; }

		public void DataBind ()
		{
		}

		#endregion

		#region IFocusable implementation

		public bool AutoFocus { get; set; }

		public string Keyboard { get; set; }

		#endregion

		#region IApplicationContextAware implementation

		public void SetApplicationContext (object applicationContext)
		{
			_applicationContext = (ApplicationContext)applicationContext;
		}

		#endregion

		#region IValidatable implementation

		public bool Validate ()
		{
			bool result = false;
			string msg = string.Empty;

			string text = _view != null ? _view.Text : _text;

			if (this.Length > 0 && text.Length > this.Length)
				msg = D.TEXT_TOO_LONG;
			else if (this.Required && string.IsNullOrWhiteSpace (text))
				msg = D.FIELD_SHOULDNT_BE_EMPTY;
			else if (!Regex.IsMatch (text, this.Mask))
				msg = D.INVALID_VALUES;
			else
				result = true;

			if (_view != null) {
				if (result) {
					_view.RightViewMode = UITextFieldViewMode.Never;
					_view.RightView = null;
				} else {
					_view.RightViewMode = UITextFieldViewMode.Always;
					UIImageView img = new UIImageView (new RectangleF (0, 0, _view.Frame.Height * 3 / 4, _view.Frame.Height * 3 / 4));
					img.Image = new UIImage ("warning.png");
					img.ContentMode = UIViewContentMode.ScaleAspectFit;
					_view.RightView = img;
				}
			}

			return result;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					_applicationContext = null;
				}

				if (_view != null) {
					_view.EditingChanged -= HandleEditingChanged;
					_view.TouchUpOutside -= HandleTouchUpOutside;
					_view.EditingDidBegin -= HandleEditingDidBegin;
					_view.Ended -= HandleEditingChanged;	
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		void HandleEditingChanged (object sender, EventArgs e)
		{
			_view.RightViewMode = UITextFieldViewMode.Never;
			_view.RightView = null;

			if (Value != null && !ApplicationContext.Busy)
				Value.ControlChanged (this.Text);

			if (OnChange != null) {
				OnChange.Execute ();
			}
		}

		void HandleEditingEnded (object sender, EventArgs e)
		{
			HandleEditingChanged (sender, e);

			if (OnLostFocus != null) {
				OnLostFocus.Execute ();
			}
		}

		void HandleTouchUpOutside (object sender, EventArgs e)
		{
			_view.ResignFirstResponder ();
		}

		void HandleEditingDidBegin (object sender, EventArgs e)
		{
			CloseModalWindows ();
			if (OnGetFocus != null)
				OnGetFocus.Execute ();			

			if(_view != null)
				_view.BeginInvokeOnMainThread (() => {
					if(_view != null) {
						_view.SelectAll (new MonoTouch.Foundation.NSObject ());
						_view.Selected = true;
					}
				});
		}

		void SetupPlaceholder (string text)
		{
			text = text ?? string.Empty;
			if (_placeholderColor != null) {
				_view.AttributedPlaceholder = new MonoTouch.Foundation.NSAttributedString (text, null, _placeholderColor);
			} else
				_view.Placeholder = text;
		}

		public class NativeTextField: UITextField
		{
			public	float PaddingTop { get; set; }

			public	float PaddingLeft { get; set; }

			public	float PaddingBottom { get; set; }

			public	float PaddingRight { get; set; }

			public override RectangleF TextRect (RectangleF forBounds)
			{
				RectangleF newRect = new RectangleF (forBounds.X + PaddingLeft, forBounds.Y + PaddingTop, forBounds.Width - PaddingLeft - PaddingRight, forBounds.Height - PaddingTop - PaddingBottom);
				return base.TextRect (newRect);
			}

			public override RectangleF EditingRect (RectangleF forBounds)
			{
				RectangleF newRect = new RectangleF (forBounds.X + PaddingLeft, forBounds.Y + PaddingTop, forBounds.Width - PaddingLeft - PaddingRight, forBounds.Height - PaddingTop - PaddingBottom);
				return base.EditingRect (newRect);
			}
		}
	}
}
