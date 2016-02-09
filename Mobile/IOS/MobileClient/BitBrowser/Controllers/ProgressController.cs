using System;
using MonoTouch.UIKit;
using BitMobile.Utilities.Translator;
using System.Drawing;
using BitMobile.Application;

namespace BitMobile.IOS
{
	public class ProgressController : StartScreenController
	{
		bool _disposed = false;
		UITextView _message;
		UIProgressView _indicator;
		UILabel _progress;

		public ProgressController (ApplicationSettings settings)
			: base (null)
		{
			_settings = settings;
		}

		protected override void LayoutContent (UIView container, ContentSet content, LayoutSet layout)
		{
			_message = new UITextView ();
			_message.Frame = new RectangleF (layout.margin, layout.offset + 2, layout.contentWidth, 40);
			_message.Editable = false;
			_message.Text = D.PLAESE_WAIT_DATA_IS_LOADED;
			_message.TextColor = LogonController.GRAY;
			_message.BackgroundColor = UIColor.Clear;
			_message.TextAlignment = UITextAlignment.Center;
			_message.Font = UIFont.FromName ("Arial", 12);
			container.AddSubview (_message);

			UIView indicatorContainer = new UIView ();
			indicatorContainer.Frame = new RectangleF (layout.margin, _message.Frame.Bottom + 2, layout.contentWidth, 20);
			indicatorContainer.BackgroundColor = UIColor.White;
			indicatorContainer.Layer.BorderColor = LogonController.GRAY.CGColor;
			indicatorContainer.Layer.BorderWidth = 1;
			indicatorContainer.Layer.CornerRadius = layout.cornerRadius;
			indicatorContainer.Layer.MasksToBounds = true;
			indicatorContainer.ClipsToBounds = true;

			_indicator = new UIProgressView ();
			_indicator.Frame = new RectangleF (10, 8, indicatorContainer.Frame.Width - 20, indicatorContainer.Frame.Height - 15);
			_indicator.TintColor = BaseColor;
			indicatorContainer.AddSubview (_indicator);
			container.AddSubview (indicatorContainer);

			_progress = new UILabel ();
			_progress.Frame = new RectangleF (layout.margin, indicatorContainer.Frame.Bottom + 2, layout.contentWidth, 40);
			_progress.BackgroundColor = UIColor.Clear;
			_progress.TextColor = LogonController.GRAY;
			_progress.Text = "";
			_progress.TextAlignment = UITextAlignment.Center;
			container.AddSubview (_progress);

		}

		public void UpdateStatus (int total, int process)
		{
			if (process > 0) {
				// With big data we have numeric overflow situation
				decimal proc = process;
				decimal tot = total;
				decimal div = proc / tot;

				int percent = (int)(div * 100);

				if (BitMobile.Application.ApplicationContext.Context.Settings.DevelopModeEnabled)
					_progress.Text = String.Format ("{0}Kb", process / 1024);
				else
					_progress.Text = String.Format ("{0}%", percent);
			}

			float progress = process / (float)total;
			_indicator.SetProgress (progress, true);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (!_disposed) {
				if (_message != null)
					_message.Dispose ();
				_message = null;
				if (_indicator != null)
					_indicator.Dispose ();
				_indicator = null;
				if (_progress != null)
					_progress.Dispose ();
				_progress = null;

				_disposed = true;
			}
		}
	}
}

