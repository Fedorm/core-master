using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Controls.StyleSheet;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;
using System.IO;

namespace BitMobile.Controls
{
	public class Button : Control<UIButton>, IApplicationContextAware, IImageContainer
	{
		protected internal ApplicationContext _applicationContext = null;
		UIImage _backgroundImageCache = null;
		UIColor _textColor;
		string _text;
		string _onEvent = "null";
		bool _disposed = false;

		public Button ()
		{	
		}

		public ActionHandler OnClickAction { get; set; }

		public ActionHandlerEx OnClick { get; set; }

		public String Text {
			get {
				if (_view != null) {
					return this._view.TitleLabel.Text;
				}
				return _text;

			}
			set {
				if (_view != null) {
					this._view.SetTitle (value, UIControlState.Normal);
				}
				_text = value;
			}
		}

		public String OnEvent {
			get {
				return _onEvent;
			}
			set {
				_onEvent = value;
				_applicationContext.SubscribeEvent (value, InvokeClick);
			}
		}

		public override UIView CreateView ()
		{
			_view = new UIButton (UIButtonType.Custom);
			_view.TouchUpInside += Button_Click;

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);


			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			// background color, background image, borders
			if (InitBackgroundImage (stylesheet))
				_view.SetBackgroundImage (_backgroundImageCache, UIControlState.Normal);
			else
				style.SetBackgroundSettings (this);

			// font
			UIFont f = style.Font (this, styleBound.Height);
			if (f != null)
				_view.Font = f;

			// text color
			_textColor = style.ColorOrClear<Color> (this);
			_view.SetTitleColor (_textColor, UIControlState.Normal);

			// selected-color
			var selectedColor = style.Color<SelectedColor> (this);
			if (selectedColor != null)
				_view.SetTitleColor (selectedColor, UIControlState.Highlighted);

			_view.SetTitle (_text, UIControlState.Normal);
			return styleBound;
		}

		#region IApplicationContextAware implementation

		public void SetApplicationContext (object applicationContext)
		{
			_applicationContext = (ApplicationContext)applicationContext;
		}

		#endregion

		#region IImageContainer implementation

		public bool InitializeImage (BitMobile.Controls.StyleSheet.StyleSheet stylesheet)
		{
			if (InitBackgroundImage (stylesheet)) {

				this.ImageWidth = (int)_backgroundImageCache.Size.Width;
				this.ImageHeight = (int)_backgroundImageCache.Size.Height;
				return true;					
			}
			return false;
		}

		public int ImageWidth {	get; set; }

		public int ImageHeight { get; set; }

		#endregion

		protected virtual bool InvokeClick ()
		{
			if (OnClick != null || OnClickAction != null) {
				CloseModalWindows ();
				EndEditing ();
			}

			if (OnClick != null) {
				OnClick.Execute ();
				return true;
			}

			if (OnClickAction != null) {
				OnClickAction.Execute ();
				return true;
			}
			return false;
		}

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					_applicationContext = null;
				}

				if (_backgroundImageCache != null)
					_backgroundImageCache.Dispose ();
				_backgroundImageCache = null;

				if (_view != null)
					_view.TouchUpInside -= Button_Click;

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		void Button_Click (object sender, EventArgs e)
		{
			InvokeClick ();
		}

		bool InitBackgroundImage (StyleSheet.StyleSheet stylesheet)
		{
			if (_backgroundImageCache == null) {
				String imgPath = stylesheet.GetHelper<StyleHelper> ().BackgroundImage (this);
				if (imgPath != null) {
					Stream imgStream = _applicationContext.DAL.GetImageByName (imgPath);
					if (imgStream != null)
						_backgroundImageCache = UIImage.LoadFromData (MonoTouch.Foundation.NSData.FromStream (imgStream));					
				}
			} 
			return _backgroundImageCache != null;
		}
	}
}
