using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BitMobile.Common.Device.Providers;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class DialogProvider : IDialogProvider
    {
        private readonly List<UIActionSheet> _actionSheets = new List<UIActionSheet>();
        private readonly List<UIAlertView> _alertViews = new List<UIAlertView>();
        private readonly IOSApplicationContext _context;

        public DialogProvider(IOSApplicationContext context)
        {
            _context = context;
        }

        #region IDialogProvider implementation

        public Task<int> Alert(string message, IDialogButton positive, IDialogButton negative, IDialogButton neutral)
        {
            return ShowDialog(message, positive, negative, neutral);
        }

        public Task Message(string message, IDialogButton ok)
        {
            return ShowDialog(message, ok);
        }

        public async Task<bool> Ask(string message, IDialogButton positive, IDialogButton negative)
        {
            int result = await ShowDialog(message, positive, negative);
            return result == 0;
        }

        public Task<IDialogAnswer<DateTime>> DateTime(string caption, DateTime current, IDialogButton positive,
            IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, UIDatePickerMode.DateAndTime, current, positive, negative);
        }

        public Task<IDialogAnswer<DateTime>> Date(string caption, DateTime current, IDialogButton positive,
            IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, UIDatePickerMode.Date, current, positive, negative);
        }

        public Task<IDialogAnswer<DateTime>> Time(string caption, DateTime current, IDialogButton positive,
            IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, UIDatePickerMode.Time, current, positive, negative);
        }

        public Task<IDialogAnswer<object>> Choose(string caption, KeyValuePair<object, string>[] items, int index,
            IDialogButton positive, IDialogButton negative)
        {
            return ShowSelectionDialog(caption, items, positive, negative);
        }

        #endregion

        private Task<int> ShowDialog(string message, IDialogButton positive, IDialogButton negative = null,
            IDialogButton neutral = null)
        {
            var tsc = new TaskCompletionSource<int>();

            string cancelButtonTitle = positive.Caption;

            var otherButtons = new List<string>(2);
            if (negative != null)
                otherButtons.Add(negative.Caption);
            if (neutral != null)
                otherButtons.Add(neutral.Caption);

            var alertView = new UIAlertView("", message, null, cancelButtonTitle, otherButtons.ToArray());

            alertView.Delegate = new AlertViewDelegate(_alertViews, tsc);

            alertView.Show();
            _alertViews.Add(alertView);

            return tsc.Task;
        }

        private Task<IDialogAnswer<DateTime>> ShowDateTimeDialog(string caption, UIDatePickerMode mode, DateTime current,
            IDialogButton positive, IDialogButton negative)
        {
            var tcs = new TaskCompletionSource<IDialogAnswer<DateTime>>();
            var version = new Version(UIDevice.CurrentDevice.SystemVersion);

            if (version.Major >= 7)
            {
                var picker = new DatePicker();
                picker.Title = caption;
                picker.NativeDatePicker.Mode = mode;
                picker.NativeDatePicker.Date = current;
                picker.DoneTitle = positive.Caption;
                picker.CancelTitle = negative.Caption;
                picker.Click += (sender, e) =>
                {
                    if (tcs.Task.Status < TaskStatus.RanToCompletion)
                    {
                        NSDate dateTime = (sender as DatePicker).NativeDatePicker.Date;
                        DateTime result = dateTime != null
                            ? System.DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime()
                            : System.DateTime.MinValue;
                        if (e.ButtonIndex == 0)
                            tcs.SetResult(new DialogAnswer<DateTime>(true, result));
                        else if (e.ButtonIndex == 1)
                            tcs.SetResult(new DialogAnswer<DateTime>(false, result));
                    }
                };

                picker.Show(_context.MainController.View,
                    (ScreenController) _context.MainController.VisibleViewController);
            }
            else
            {
                var alertView = new UIAlertView(caption, "", null, positive.Caption, negative.Caption);
                alertView.Show();
                _alertViews.Add(alertView);

                var datePicker = new UIDatePicker();
                datePicker.Frame = new RectangleF(10, alertView.Bounds.Size.Height, 270, 150);
                datePicker.Mode = UIDatePickerMode.Date;
                datePicker.Date = current;
                alertView.AddSubview(datePicker);

                var timePicker = new UIDatePicker();
                timePicker.Frame = new RectangleF(10, alertView.Bounds.Size.Height + 150 + 20, 270, 150);
                timePicker.Mode = UIDatePickerMode.Time;
                timePicker.Date = current;
                alertView.AddSubview(timePicker);

                alertView.Clicked += (sender, e) =>
                {
                    DateTime date = System.DateTime.SpecifyKind(datePicker.Date, DateTimeKind.Unspecified);
                    DateTime time = System.DateTime.SpecifyKind(timePicker.Date, DateTimeKind.Utc).ToLocalTime();
                    var result = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                    if (e.ButtonIndex == 0)
                        tcs.SetResult(new DialogAnswer<DateTime>(true, result));
                    else if (e.ButtonIndex == 1)
                        tcs.SetResult(new DialogAnswer<DateTime>(false, result));

                    var av = (UIAlertView) sender;
                    _alertViews.Remove(av);
                    av.Dispose();
                };

                alertView.Bounds = new RectangleF(0, 0, 290, alertView.Bounds.Size.Height + 150 + 150 + 20 + 30);
            }
            return tcs.Task;
        }

        private Task<IDialogAnswer<object>> ShowSelectionDialog(string caption, KeyValuePair<object, string>[] items,
            IDialogButton positive, IDialogButton negative)
        {
            var tcs = new TaskCompletionSource<IDialogAnswer<object>>();
            string[] rows = items.Select(val => val.Value).ToArray();

            var actionSheet = new UIActionSheet(caption, null, negative.Caption, null, rows);
            actionSheet.Delegate = new ActionSheetDelegate(items, _actionSheets, tcs);
            actionSheet.ShowInView(_context.MainController.View);
            _actionSheets.Add(actionSheet);
            return tcs.Task;
        }

        private class ActionSheetDelegate : UIActionSheetDelegate
        {
            private readonly KeyValuePair<object, string>[] _items;
            private readonly List<UIActionSheet> _list;
            private readonly TaskCompletionSource<IDialogAnswer<object>> _tcs;
            private bool _dismissed;

            public ActionSheetDelegate(KeyValuePair<object, string>[] items, List<UIActionSheet> list,
                TaskCompletionSource<IDialogAnswer<object>> tcs)
            {
                _items = items;
                _list = list;
                _tcs = tcs;
            }

            public override void WillPresent(UIActionSheet actionSheet)
            {
                if (UIDevice.CurrentDevice.Model.Contains("iPad"))
                {
                    actionSheet.BackgroundColor = UIColor.White;
                    foreach (UIView subview in actionSheet.Subviews)
                    {
                        subview.BackgroundColor = UIColor.White;
                    }
                }
            }

            public override void Dismissed(UIActionSheet actionSheet, int buttonIndex)
            {
                if (!_dismissed)
                {
                    _dismissed = true;
                    // if user uses gallery, camera, etc, application will crash, because UINavigationController does not presented
                    if (_items.Length > buttonIndex /*because last button is cancel*/)
                        _tcs.SetResult(new DialogAnswer<object>(true, _items[buttonIndex].Key));
                    else
                        _tcs.SetResult(new DialogAnswer<object>(false, null));

                    _list.Remove(actionSheet);
                    actionSheet.Dispose();
                }
            }
        }

        private class AlertViewDelegate : UIAlertViewDelegate
        {
            private readonly List<UIAlertView> _alertViews;
            private readonly TaskCompletionSource<int> _tcs;
            private bool _dismissed;

            public AlertViewDelegate(List<UIAlertView> alertViews, TaskCompletionSource<int> tcs)
            {
                _alertViews = alertViews;
                _tcs = tcs;
            }

            public override void Dismissed(UIAlertView alertview, int buttonIndex)
            {
                if (!_dismissed)
                {
                    _dismissed = true;
                    // if user uses gallery, camera, etc, application will crash, because UINavigationController does not presented
                    _tcs.SetResult(buttonIndex);

                    _alertViews.Remove(alertview);
                    alertview.Dispose();
                }
            }
        }

        private class DialogAnswer<T> : IDialogAnswer<T>
        {
            public DialogAnswer(bool positive, T result)
            {
                Result = result;
                Positive = positive;
            }

            public bool Positive { get; private set; }

            public T Result { get; private set; }
        }
    }
}