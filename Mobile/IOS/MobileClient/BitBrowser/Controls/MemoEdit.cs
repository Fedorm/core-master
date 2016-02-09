using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;
using BitMobile.Controls.StyleSheet;
using System.Text.RegularExpressions;

namespace BitMobile.Controls
{
	public class MemoEdit : Control<UITextView>, IDataBind, IFocusable, IApplicationContextAware
	{
		ApplicationContext _applicationContext = null;
		string _text;
		string _placeholder;
		UILabel _placeholderView;
		bool _disposed = false;

		public MemoEdit ()
		{     
			this.Keyboard = "auto";
			TextLength = 0;

			TabOrderManager.Current.Add (this);
		}

		public String Text {
			get {
				if (_view != null)
					return _view.Text;
				return _text;
			}
			set {
				_text = value;
				if (_view != null) {
					_view.Text = _text;
					HandlePlaceholderVisiblity ();
				}
			}
		}

		public String Placeholder {
			get {
				if (_placeholderView != null)
					return _placeholderView.Text;
				return _placeholder;
			}
			set {
				_placeholder = value;
				if (_placeholderView != null) {
					_placeholderView.Text = _placeholder;
					_placeholderView.SizeToFit ();
				}
			}
		}

		public ActionHandlerEx OnChange { get; set; }

		public ActionHandlerEx OnGetFocus { get; set; }

		public ActionHandlerEx OnLostFocus { get; set; }

		public int TextLength { get; set; }

		public void SetFocus ()
		{
			if (_view != null) {
				_applicationContext.InvokeOnMainThread (() => _view.BecomeFirstResponder ());
			} else
				AutoFocus = true;
		}

		public override UIView CreateView ()
		{
			_view = new UITextView ();
			_view.Text = _text;
			_view.Editable = true;
			_view.Changed += HandleChanged;
			_view.Ended += HandleChanged;
			_view.Ended += HandleEnded;
			_view.Started += HandleStarted;
			if (ApplicationContext.OSVersion.Major >= 7)
				_view.TextContainer.LineFragmentPadding = 0;

			_view.TextContainer.MaximumNumberOfLines = 1;
			switch (this.Keyboard.ToLower ()) {
			case "auto":
				if (Value != null && Value.IsNumeric ())
				if (UIDevice.CurrentDevice.Model.Contains ("iPhone"))
					_view.KeyboardType = UIKeyboardType.DecimalPad;
				else
					_view.KeyboardType = UIKeyboardType.NumberPad;
				else
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

			SetupPlaceholder ();

			if (this.AutoFocus) {
				_view.BecomeFirstResponder ();
			}

			TabOrderManager.Current.AttachAccessory (this);

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			// background color, borders
			style.SetBackgroundSettings (this);

			// text color
			_view.TextColor = style.ColorOrClear<Color> (this);

			// placeholder color
			UIColor	placeholderColor = style.Color<PlaceholderColor> (this);
			if (placeholderColor != null)
				_placeholderView.TextColor = placeholderColor;			

			// font
			UIFont f = style.Font (this, styleBound.Height);
			if (f != null)
				_view.Font = f;

			// padding
			float pl = style.Padding<PaddingLeft> (this, styleBound.Width);
			float pt = style.Padding<PaddingTop> (this, styleBound.Height);
			float pr = style.Padding<PaddingRight> (this, styleBound.Width);
			float pb = style.Padding<PaddingBottom> (this, styleBound.Height);
			if (ApplicationContext.OSVersion.Major >= 7)
				_view.TextContainerInset = new UIEdgeInsets (pt, pl, pb, pr);

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

			_placeholderView.Font = _view.Font;
			_placeholderView.Frame = new System.Drawing.RectangleF (pl, pt, maxBound.Width - (pl + pr), maxBound.Height - (pt + pb));
			_placeholderView.SizeToFit ();

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

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (_view != null) {
					_view.Changed -= HandleChanged;
					_view.Ended -= HandleChanged;
					_view.Ended -= HandleEnded;
					_view.Started -= HandleStarted;
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		void HandleChanged (object sender, EventArgs e)
		{
			HandlePlaceholderVisiblity ();
			if (this.Value != null && !ApplicationContext.Busy)
				this.Value.ControlChanged (this.Text);

			if (OnChange != null) {
				OnChange.Execute ();
			}
		}

		void HandleEnded (object sender, EventArgs e)
		{
			_view.ResignFirstResponder ();

			if (OnLostFocus != null)
				OnLostFocus.Execute ();
		}

		void HandleStarted (object sender, EventArgs e)
		{
			CloseModalWindows ();
			if (OnGetFocus != null)
				OnGetFocus.Execute ();

			if(_view != null)
				_view.BeginInvokeOnMainThread (() => {		
					if(_view != null)
						_view.SelectAll (new MonoTouch.Foundation.NSObject ());
				});
		}

		void SetupPlaceholder ()
		{
			_placeholderView = new UILabel ();
			_placeholderView.Text = _placeholder;
			_placeholderView.BackgroundColor = UIColor.Clear;
			_placeholderView.TextColor = UIColor.LightGray;
			_view.AddSubview (_placeholderView);
			HandlePlaceholderVisiblity ();
		}

		void HandlePlaceholderVisiblity ()
		{
			_placeholderView.Hidden = !string.IsNullOrEmpty (Text);					
		}
	}
}