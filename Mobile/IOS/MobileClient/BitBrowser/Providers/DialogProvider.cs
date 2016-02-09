using System;
using BitMobile.Common;
using MonoTouch.UIKit;
using BitMobile.Controls;
using System.Collections.Generic;
using System.Linq;

namespace BitMobile.IOS
{
	public class DialogProvider : IDialogProvider
	{
		ApplicationContext _context;
		List<UIAlertView> _alertViews = new List<UIAlertView> ();
		List<UIActionSheet> _actionSheets = new List<UIActionSheet> ();

		public DialogProvider (ApplicationContext context)
		{
			_context = context;
		}

		#region IDialogProvider implementation

		public void Alert (string message, BitMobile.Controls.DialogButton<int> positive, BitMobile.Controls.DialogButton<int> negative, BitMobile.Controls.DialogButton<int> neutral)
		{
			ShowDialog (message, positive, negative, neutral);
		}

		public void Message (string message, BitMobile.Controls.DialogButton<object> ok)
		{
			ShowDialog (message, ok);
		}

		public void Ask (string message, BitMobile.Controls.DialogButton<object> positive, BitMobile.Controls.DialogButton<object> negative)
		{
			ShowDialog (message, positive, negative);
		}

		public void DateTime (string caption, DateTime current, BitMobile.Controls.DialogButton<DateTime> positive, BitMobile.Controls.DialogButton<DateTime> negative)
		{
			ShowDateTimeDialog (caption, UIDatePickerMode.DateAndTime, current, positive, negative);
		}

		public void Date (string caption, DateTime current, BitMobile.Controls.DialogButton<DateTime> positive, BitMobile.Controls.DialogButton<DateTime> negative)
		{
			ShowDateTimeDialog (caption, UIDatePickerMode.Date, current, positive, negative);
		}

		public void Time (string caption, DateTime current, BitMobile.Controls.DialogButton<DateTime> positive, BitMobile.Controls.DialogButton<DateTime> negative)
		{
			ShowDateTimeDialog (caption, UIDatePickerMode.Time, current, positive, negative);
		}

		public void Choose (string caption, System.Collections.Generic.KeyValuePair<object, string>[] items, int index, BitMobile.Controls.DialogButton<object> positive, BitMobile.Controls.DialogButton<object> negative)
		{
			ShowSelectionDialog (caption, items, index, positive, negative);
		}

		#endregion

		public void ShowDialog<T> (string message, DialogButton<T> positive, DialogButton<T> negative = null, DialogButton<T> neutral = null)
		{
			string cancelButtonTitle = positive.Caption;

			var otherButtons = new List<string> (2);
			if (negative != null)
				otherButtons.Add(negative.Caption);
			if (neutral != null)
				otherButtons.Add(neutral.Caption);

			var alertView = new UIAlertView ("", message, null, cancelButtonTitle, otherButtons.ToArray());

			alertView.Delegate = new AlertViewDelegate<T>(_alertViews, positive, negative, neutral);

			alertView.Show ();
			_alertViews.Add (alertView);
		}

		public void ShowDateTimeDialog (string caption, UIDatePickerMode mode, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative)
		{
			Version version = new Version (UIDevice.CurrentDevice.SystemVersion);

			if (version.Major >= 7) {
				DatePicker picker = new DatePicker ();
				picker.Title = caption;
				picker.NativeDatePicker.Mode = mode;
				picker.NativeDatePicker.Date = current;
				picker.DoneTitle = positive.Caption;
				picker.CancelTitle = negative.Caption;
				picker.Click += (object sender, UIButtonEventArgs e) => {
					var dateTime = (sender as DatePicker).NativeDatePicker.Date;
					DateTime result = dateTime != null ? System.DateTime.SpecifyKind (dateTime, DateTimeKind.Utc).ToLocalTime () : System.DateTime.MinValue;
					if (e.ButtonIndex == 0)
						positive.Execute (result);
					else if (e.ButtonIndex == 1)
						negative.Execute (result);
				};

				picker.Show (_context.MainController.View, (ScreenController)_context.MainController.VisibleViewController);

			} else {
				var alertView = new UIAlertView (caption, "", null, positive.Caption, negative.Caption);
				alertView.Show ();
				_alertViews.Add (alertView);

				UIDatePicker datePicker = new UIDatePicker ();
				datePicker.Frame = new System.Drawing.RectangleF (10, alertView.Bounds.Size.Height, 270, 150);
				datePicker.Mode = UIDatePickerMode.Date;
				datePicker.Date = current;
				alertView.AddSubview (datePicker);

				UIDatePicker timePicker = new UIDatePicker ();
				timePicker.Frame = new System.Drawing.RectangleF (10, alertView.Bounds.Size.Height + 150 + 20, 270, 150);
				timePicker.Mode = UIDatePickerMode.Time;
				timePicker.Date = current;
				alertView.AddSubview (timePicker);

				alertView.Clicked += (object sender, UIButtonEventArgs e) => {
					DateTime date = System.DateTime.SpecifyKind (datePicker.Date, DateTimeKind.Unspecified);
					DateTime time = System.DateTime.SpecifyKind (timePicker.Date, DateTimeKind.Utc).ToLocalTime ();
					DateTime result = new DateTime (date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
					if (e.ButtonIndex == 0)
						positive.Execute (result);
					else if (e.ButtonIndex == 1)
						negative.Execute (result);

					var av = (UIAlertView)sender;
					_alertViews.Remove(av);
					av.Dispose ();
				};

				alertView.Bounds = new System.Drawing.RectangleF (0, 0, 290, alertView.Bounds.Size.Height + 150 + 150 + 20 + 30);
			}
		}

		public void ShowSelectionDialog (string caption, KeyValuePair<object, string>[] items, int index, DialogButton<object> positive, DialogButton<object> negative)
		{
			string[] rows = items.Select (val => val.Value).ToArray ();

			UIActionSheet actionSheet = new UIActionSheet (caption, null, negative.Caption, null, rows);
			actionSheet.Delegate = new ActionSheetDelegate (items, _actionSheets, positive, negative);
			actionSheet.ShowInView (_context.MainController.View);
			_actionSheets.Add (actionSheet);
		}

		class AlertViewDelegate<T>: UIAlertViewDelegate
		{
			List<UIAlertView> _alertViews;
			DialogButton<T> _positive;
			DialogButton<T> _negative; 
			DialogButton<T> _neutral;

			public AlertViewDelegate (List<UIAlertView> alertViews, DialogButton<T> positive, DialogButton<T> negative, DialogButton<T> neutral)
			{
				_alertViews = alertViews;
				_positive = positive;
				_negative = negative;
				_neutral = neutral;
			}

			public override void Dismissed (UIAlertView alertview, int buttonIndex)
			{
				// if user uses gallery, camera, etc, application will crash, because UINavigationController does not presented
				if (buttonIndex == 0)
					_positive.Execute ();
				else if (buttonIndex == 1)
					_negative.Execute ();
				else if (buttonIndex == 2)
					_neutral.Execute ();	

				_alertViews.Remove(alertview);
				alertview.Dispose ();
			}
		}

		class ActionSheetDelegate :UIActionSheetDelegate
		{
			KeyValuePair<object, string>[] _items;
			List<UIActionSheet> _list;
			DialogButton<object> _positive;
			DialogButton<object> _negative;

			public ActionSheetDelegate (KeyValuePair<object, string>[] items, List<UIActionSheet> list, DialogButton<object> positive, DialogButton<object> negative)
			{
				_items = items;
				_list = list;
				_positive = positive;
				_negative = negative;
			}

			public override void WillPresent (UIActionSheet actionSheet)
			{
				if (UIDevice.CurrentDevice.Model.Contains ("iPad")) {
					actionSheet.BackgroundColor = UIColor.White;
					foreach (var subview in actionSheet.Subviews) {
						subview.BackgroundColor = UIColor.White;
					}
				}
			}

			public override void Dismissed (UIActionSheet actionSheet, int buttonIndex)
			{
				// if user uses gallery, camera, etc, application will crash, because UINavigationController does not presented
				if (_items.Length > buttonIndex /*because last button is cancel*/)
					_positive.Execute (_items [buttonIndex].Key);
				else
					_negative.Execute ();

				_list.Remove (actionSheet);
				actionSheet.Dispose ();
			}
		}
	}
}

