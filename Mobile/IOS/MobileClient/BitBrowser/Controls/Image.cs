using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.Utilities.IO;
using BitMobile.Controls.StyleSheet;
using BitMobile.UI;
using BitMobile.Application;
using BitMobile.IOS;
using System.Drawing;

namespace BitMobile.Controls
{
	[Synonym ("img")]
	public class Image : Control<UIImageView>, IApplicationContextAware, IImageContainer
	{
		IApplicationContext _context = null;
		UIImage _backgroungImageCache = null;
		UIImage _selectedImage;
		bool _disposed = false;

		public Image ()
		{	
		}

		public string Source { get; set; }

		public override UIView CreateView ()
		{
			_view = new UIImageView ();

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			// background image
			if (InitImage (stylesheet)) {
				_view.Image = _backgroungImageCache;

				// selected-color
				var selectedColor = style.Color<SelectedColor> (this);
				if (selectedColor != null)
					_selectedImage = GetFilteredImage (_backgroungImageCache, selectedColor);
			}

			return styleBound;
		}

		public override void AnimateTouch (TouchEventType touch)
		{
			if (_selectedImage != null) {
				switch (touch) {
				case TouchEventType.Begin:
					_view.Image = _selectedImage;
					break;
				case TouchEventType.Cancel:
				case TouchEventType.End:
					UIView.BeginAnimations (null);
					UIView.SetAnimationDuration (0.1);
					_view.Image = _backgroungImageCache;
					UIView.CommitAnimations ();
					break;
				}
			}
		}

		#region IApplicationContextAware implementation

		public void SetApplicationContext (object applicationContext)
		{
			_context = (IApplicationContext)applicationContext;
		}

		#endregion

		#region IImageContainer implementation

		public int ImageWidth {	get; set; }

		public int ImageHeight { get; set; }

		public bool InitializeImage (BitMobile.Controls.StyleSheet.StyleSheet stylesheet)
		{
			bool result = false;

			if (InitImage (stylesheet)) {
				ImageWidth = (int)_backgroungImageCache.Size.Width;
				ImageHeight = (int)_backgroungImageCache.Size.Height;
				result = true;
			}

			return result;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					_context = null;
				}

				if (_backgroungImageCache != null)
					_backgroungImageCache.Dispose ();
				_backgroungImageCache = null;	

				if (_selectedImage != null) {
					_selectedImage.Dispose ();
					_selectedImage = null;
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		UIImage GetFilteredImage (UIImage image, UIColor color)
		{
			RectangleF rect = new RectangleF (0, 0, image.Size.Width, image.Size.Height);
			UIGraphics.BeginImageContext (rect.Size);
			var context = UIGraphics.GetCurrentContext ();
			context.ClipToMask (rect, image.CGImage);
			context.SetFillColorWithColor (color.CGColor);
			context.FillRect (rect);
			UIImage img = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			if (img != null)
				return new UIImage (img.CGImage, 1, UIImageOrientation.DownMirrored);
			return image;
		}

		bool InitImage (BitMobile.Controls.StyleSheet.StyleSheet stylesheet)
		{
			if (_backgroungImageCache == null) {
				if (Source != null) {
					_backgroungImageCache = UIImage.FromFile (FileSystemProvider.TranslatePath (_context.LocalStorage, Source));
				} else {
					String imgPath = stylesheet.GetHelper<StyleHelper> ().BackgroundImage (this);
					if (imgPath != null)
						_backgroungImageCache = UIImage.LoadFromData (MonoTouch.Foundation.NSData.FromStream (_context.DAL.GetImageByName (imgPath)));
				}
			}

			return _backgroungImageCache != null;
		}
	}
}
