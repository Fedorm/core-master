using System;
using BitMobile.Application;
using MonoTouch.UIKit;
using BitMobile.Utilities.Translator;
using System.Drawing;

namespace BitMobile.IOS
{
	public class StartScreenController: ScreenController
	{
		public static readonly UIColor VIOLET_RED = UIColor.FromRGB (210, 0, 126);
		public static readonly UIColor DARK_BLUE = UIColor.FromRGB (42, 45, 135);
		public static readonly UIColor GRAY = UIColor.FromRGB (194, 194, 194);
		public static readonly UIColor RED = UIColor.FromRGB (247, 71, 71);
		public static readonly UIColor BLUE = UIColor.FromRGB (67, 172, 253);
		public static readonly UIColor YELLOW = UIColor.FromRGB (230, 137, 27);
		protected ApplicationSettings _settings;

		public StartScreenController (ApplicationSettings settings)
			: base (null)
		{
			_settings = settings;
		}

		protected UIColor BaseColor {
			get {
				switch (_settings.CurrentSolutionType) {
				case SolutionType.BitMobile:
					return DARK_BLUE;
				case SolutionType.SuperAgent:
					return BLUE;
				case SolutionType.LandSuperService:
				case SolutionType.SuperService:
					return YELLOW;
				default:
					return DARK_BLUE;
				}
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var af = UIScreen.MainScreen.ApplicationFrame;
			ContentSet content = new ContentSet ();
			LayoutSet layout = new LayoutSet ();

			// prepare solution data
			content.borderColor = BaseColor;
			content.buttonColor = BaseColor;
			if (_settings.CurrentSolutionType == SolutionType.BitMobile) {
				content.topImg = new UIImage ("BitMobileTop.png");
				content.botImg = new UIImage ("BitMobileBottom.png");
				content.borderColor = VIOLET_RED;
			} else {
				content.lockImg = GetFilteredImage (content.lockImg, BaseColor);
				content.usernameImg = GetFilteredImage (content.usernameImg, BaseColor);
				content.passwordImg = GetFilteredImage (content.passwordImg, BaseColor);

				switch (_settings.CurrentSolutionType) {
				case SolutionType.SuperAgent:
					content.topImg = new UIImage ("SuperAgentTop.png");
					content.botImg = new UIImage ("SuperAgentBottom.png");
					content.caption1Text = D.BIT_CATCH1;
					content.logoImg = new UIImage ("SuperAgentLogo.png");
					content.caption2Text = D.BIT_CATCH2;
					break;
				case SolutionType.SuperService:
					content.topImg = new UIImage ("SuperServiceTop.png");
					content.botImg = new UIImage ("SuperServiceBottom.png");
					content.caption1Text = D.SUPER_SERVICE1;
					content.logoImg = new UIImage ("SuperServiceLogo.png");
					content.caption2Text = D.SUPER_SERVICE2;
					break;
				case SolutionType.LandSuperService:
					content.topImg = new UIImage ("SuperServiceTop.png");
					content.botImg = new UIImage ("SuperServiceBottom.png");
					content.logoImg = new UIImage ("LandSuperServiceLogo.png");
					content.customerLogo = new UIImage ("LandLogo.png");
					break;
				}
			}

			// start building
			layout.offset = 0;

			UIView container = new UIView ();
			container.Frame = UIScreen.MainScreen.ApplicationFrame;
			container.BackgroundColor = UIColor.White;

			layout.cornerRadius = 6;
			layout.contentWidth = af.Width - 20 > 300 ? 300 : af.Width - 20;
			layout.margin = (af.Width - layout.contentWidth) / 2;
			float topHeight = (af.Left + af.Width) * content.topImg.Size.Height / content.topImg.Size.Width;

			using (UIImageView imageTop = new UIImageView ()) {
				imageTop.Image = content.topImg;
				imageTop.Frame = new RectangleF (0, 0, af.Left + af.Width, topHeight);
				container.AddSubview (imageTop);
			}

			using (UIImageView imageBot = new UIImageView ()) {
				imageBot.Image = content.botImg;
				float botHeight = (af.Left + af.Width) * imageBot.Image.Size.Height / imageBot.Image.Size.Width;
				imageBot.Frame = new RectangleF (0, (af.Top + af.Height) - botHeight, af.Left + af.Width, botHeight);
				container.AddSubview (imageBot);
				layout.offset = imageBot.Frame.Top;
			}

			float captionTopMargin = 20 + topHeight / 6;
			if (content.customerLogo != null)
				captionTopMargin -= 20;
			float captionHeight = 40;
			if (_settings.CurrentSolutionType == SolutionType.BitMobile) {
				using (UILabel caption = new UILabel ()) {
					caption.Frame = new RectangleF (layout.margin, captionTopMargin, layout.contentWidth, captionHeight);
					caption.BackgroundColor = UIColor.Clear;
					caption.TextColor = UIColor.White;
					caption.Text = D.BIT_MOBILE;
					caption.TextAlignment = UITextAlignment.Center;
					caption.Font = UIFont.FromName ("Arial-BoldMT", 25);
					container.AddSubview (caption);
				}
			} else {
				float textPadding = 4;
				float logoHeight = 40;
				float logoWidth = logoHeight * content.logoImg.Size.Width / content.logoImg.Size.Height;
				float captionWidth = (layout.contentWidth - logoWidth - textPadding) / 2;

				float captionOffset = layout.margin;
				using (UILabel caption = new UILabel ()) {
					caption.Frame = new RectangleF (captionOffset, captionTopMargin, captionWidth - textPadding, captionHeight);
					caption.BackgroundColor = UIColor.Clear;
					caption.TextColor = UIColor.White;
					caption.TextAlignment = UITextAlignment.Right;
					caption.Font = UIFont.FromName ("Arial-BoldMT", 25);
					caption.Text = content.caption1Text;
					container.AddSubview (caption);
					captionOffset = caption.Frame.Right;				
				}
				using (UIImageView	imageLogo = new UIImageView ()) {
					imageLogo.Frame = new RectangleF (captionOffset + textPadding, captionTopMargin, logoWidth, logoHeight);
					imageLogo.Image = content.logoImg;
					container.AddSubview (imageLogo);
					captionOffset = imageLogo.Frame.Right;
				}
				using (UILabel	caption = new UILabel ()) {
					caption.Frame = new RectangleF (captionOffset + textPadding, captionTopMargin, captionWidth, captionHeight);
					caption.BackgroundColor = UIColor.Clear;
					caption.TextColor = UIColor.White;
					caption.TextAlignment = UITextAlignment.Left;
					caption.Font = UIFont.FromName ("Arial-BoldMT", 25);
					caption.Text = content.caption2Text;
					container.AddSubview (caption);
				}
				if (content.customerLogo != null)
					using (UIImageView custLogo = new UIImageView ()) {
						float custLogoHeight = 40;
						float custLogoWidth = custLogoHeight * content.customerLogo.Size.Width / content.customerLogo.Size.Height;
						float custXOffset = (layout.contentWidth - custLogoWidth) / 2 + layout.margin;
						float custYOffset = captionTopMargin + logoHeight + 10;
						custLogo.Frame = new RectangleF (custXOffset, custYOffset, custLogoWidth, custLogoHeight);
						custLogo.Image = content.customerLogo;
						container.AddSubview (custLogo);
					}
			}

			using (UIImageView imageLock = new UIImageView ()) {
				imageLock.Image = content.lockImg;
				float lockHeight = 52;
				float lockWidth = lockHeight * imageLock.Image.Size.Width / imageLock.Image.Size.Height;
				imageLock.Frame = new RectangleF (((af.Left + af.Width) - lockWidth) / 2, layout.offset + 16, lockWidth, lockHeight);
				container.AddSubview (imageLock);
				layout.offset = imageLock.Frame.Bottom;
			}

			LayoutContent (container, content, layout);

			if (_settings.CurrentSolutionType != SolutionType.BitMobile) {
				float bottomTextSize = 18;
				float marginBottom = 10;
				using (UILabel bottomText = new UILabel ()) {
					bottomText.Frame = new RectangleF (layout.margin, container.Frame.Bottom - bottomTextSize * 2 - marginBottom, layout.contentWidth, bottomTextSize);
					bottomText.Text = D.EFFECTIVE_SOLUTIONS_BASED_ON_1C_FOR_BUSINESS;
					bottomText.TextAlignment = UITextAlignment.Center;
					bottomText.TextColor = UIColor.FromRGB (142, 142, 142);
					bottomText.LineBreakMode = UILineBreakMode.WordWrap;
					bottomText.Font = UIFont.FromName ("Arial", 12);
					container.AddSubview (bottomText);
				}
				using (UILabel bottomText = new UILabel ()) {
					bottomText.Frame = new RectangleF (layout.margin, container.Frame.Bottom - bottomTextSize - marginBottom, layout.contentWidth, bottomTextSize);
					bottomText.Text = D.FIRST_BIT_COPYRIGHT;
					bottomText.TextAlignment = UITextAlignment.Center;
					bottomText.TextColor = UIColor.FromRGB (184, 184, 184);
					bottomText.Font = UIFont.FromName ("Arial", 12);
					container.AddSubview (bottomText);
				}
			}

			this.View = container;
		}

		protected virtual void LayoutContent (UIView container, ContentSet content, LayoutSet layout)
		{
		}

		protected UIImage GetFilteredImage (UIImage image, UIColor color)
		{
			RectangleF rect = new RectangleF (0, 0, image.Size.Width, image.Size.Height);
			UIGraphics.BeginImageContext (rect.Size);
			var context = UIGraphics.GetCurrentContext ();
			context.ClipToMask (rect, image.CGImage);
			context.SetFillColorWithColor (color.CGColor);
			context.FillRect (rect);
			UIImage img = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return new UIImage (img.CGImage, 1, UIImageOrientation.DownMirrored);
		}

		protected class ContentSet
		{
			public UIImage topImg = null;
			public UIImage botImg = null;
			public string caption1Text = null;
			public UIImage logoImg = null;
			public string caption2Text = null;
			public UIImage lockImg = new UIImage ("Lock.png");
			public UIImage usernameImg = new UIImage ("UserName.png");
			public UIImage passwordImg = new UIImage ("Password.png");
			public UIColor borderColor = null;
			public UIColor buttonColor = null;
			public UIImage customerLogo = null;
		}

		protected class LayoutSet
		{
			public float offset;
			public float margin;
			public float contentWidth;
			public float cornerRadius;
		}
	}
}

