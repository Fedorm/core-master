using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using BitMobile.Application.Translator;
using BitMobile.Common.Device.Providers;
using BitMobile.Droid.Application;
using Orientation = Android.Widget.Orientation;

namespace BitMobile.Droid.Providers
{
    class DialogProvider : IDialogProvider
    {
        public const int DatePicker = 1989;
        public const int TimePicker = 1411;

        private readonly BaseScreen _activity;
        private readonly AndroidApplicationContext _applicationContext;

        public DialogProvider(BaseScreen activity, AndroidApplicationContext applicationContext)
        {
            _activity = activity;
            _applicationContext = applicationContext;

            Dialogs = new Stack<AlertDialog>();
        }

        public Stack<AlertDialog> Dialogs { get; private set; }

        public async Task<int> Alert(string message, IDialogButton positive, IDialogButton negative, IDialogButton neutral)
        {
            return (int)await ShowDialog(null, message, positive, negative, neutral);
        }

        public async Task Message(string message, IDialogButton ok)
        {
            await ShowDialog(null, message, ok);
        }

        public async Task<bool> Ask(string message, IDialogButton positive, IDialogButton negative)
        {
            DialogButton button = await ShowDialog(null, message, positive, negative);
            return button == DialogButton.Positive;
        }

        public Task<IDialogAnswer<DateTime>> DateTime(string caption, DateTime current, IDialogButton positive, IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, current, true, true, positive, negative);
        }

        public Task<IDialogAnswer<DateTime>> Date(string caption, DateTime current, IDialogButton positive, IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, current, true, false, positive, negative);
        }

        public Task<IDialogAnswer<DateTime>> Time(string caption, DateTime current, IDialogButton positive, IDialogButton negative)
        {
            return ShowDateTimeDialog(caption, current, false, true, positive, negative);
        }

        public Task<IDialogAnswer<object>> Choose(string caption, KeyValuePair<object, string>[] items, int index
            , IDialogButton positive, IDialogButton negative)
        {
            return ShowSelectionDialog(caption, items, index, positive, negative);
        }

        internal Task<DialogButton> ShowDialog(string caption, string message
            , string positive, string negative = null, string neutral = null)
        {
            var tsc = new TaskCompletionSource<DialogButton>();

            AlertDialog.Builder builder = CreateBuilder();
            builder.SetMessage(message);
            if (caption != null)
                builder.SetTitle(caption);
            builder.SetPositiveButton(positive, (sender, e) => tsc.SetResult(DialogButton.Positive));
            if (negative != null)
                builder.SetNegativeButton(negative, (sender, e) => tsc.SetResult(DialogButton.Negative));
            if (neutral != null)
                builder.SetNeutralButton(neutral, (sender, e) => tsc.SetResult(DialogButton.Neutral));
            builder.SetCancelable(false);

            ShowDialog(builder);

            return tsc.Task;
        }

        private async Task<DialogButton> ShowDialog(string caption, string message
            , IDialogButton positive, IDialogButton negative = null, IDialogButton neutral = null)
        {
            string negativeCaption = negative != null ? negative.Caption : null;
            string neutralCaption = neutral != null ? neutral.Caption : null;

            return await ShowDialog(caption, message, positive.Caption, negativeCaption, neutralCaption);
        }

        private Task<IDialogAnswer<DateTime>> ShowDateTimeDialog(string caption, DateTime current, bool date, bool time
            , IDialogButton positive, IDialogButton negative)
        {
            var tsc = new TaskCompletionSource<IDialogAnswer<DateTime>>();

            var builder = CreateBuilder();
            builder.SetTitle(caption);

            using (var th = new TabHost(_activity))
            {
                th.Id = Android.Resource.Id.TabHost;

                var widget = new TabWidget(_activity) { Id = Android.Resource.Id.Tabs };

                var fl = new FrameLayout(_activity) { Id = Android.Resource.Id.TabContent };
                var rll = new LinearLayout(_activity) { Orientation = Orientation.Vertical };
                rll.SetBackgroundColor(new Color(50, 100, 145));
                rll.AddView(widget);
                rll.AddView(fl);
                fl.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;

                th.AddView(rll);

                th.Setup();

                var tabContentFactory = new TabContentFactory();

                if (date)
                {
                    var dp = new DatePicker(_activity) { Id = DatePicker };
                    var minDate = new DateTime(1900, 1, 1);
                    current = current > minDate ? current : minDate;
                    dp.DateTime = current;

                    tabContentFactory.Add("date", dp);
                    var spec = th.NewTabSpec("date");
                    spec.SetContent(tabContentFactory);
                    spec.SetIndicator(CreateTabIndicator(D.DATE));
                    th.AddTab(spec);
                }
                if (time)
                {
                    var tp = new TimePicker(_activity) { Id = TimePicker };
                    tp.SetIs24HourView(new Java.Lang.Boolean(true));
                    tp.CurrentHour = new Java.Lang.Integer(current.Hour);
                    tp.CurrentMinute = new Java.Lang.Integer(current.Minute);

                    tabContentFactory.Add("time", tp);
                    var spec = th.NewTabSpec("time");
                    spec.SetContent(tabContentFactory);
                    spec.SetIndicator(CreateTabIndicator(D.TIME));
                    th.AddTab(spec);
                }

                widget.SetCurrentTab(0);
                builder.SetView(th);
            }
            builder.SetPositiveButton(positive.Caption, (sender, e) =>
            {
                DateTime result = DateTimeFromDialog(sender, current, date, time);
                tsc.SetResult(new DialogAnswer<DateTime>(true, result));
            });
            builder.SetNegativeButton(negative.Caption, (sender, e) =>
            {
                DateTime result = DateTimeFromDialog(sender, current, date, time);
                tsc.SetResult(new DialogAnswer<DateTime>(false, result));
            });
            builder.SetCancelable(false);

            ShowDialog(builder);

            return tsc.Task;
        }

        private DateTime DateTimeFromDialog(object sender, DateTime current, bool date, bool time)
        {
            var alertDialog = (AlertDialog) sender;
            
            DateTime result;
            using (var dp = alertDialog.FindViewById<DatePicker>(DatePicker))
            using (var tp = alertDialog.FindViewById<TimePicker>(TimePicker))
            {
                if (dp != null)
                    dp.ClearFocus();
                if (tp != null)
                    tp.ClearFocus();

                DateTime zeroDateTime = date & time ? current : System.DateTime.MinValue;

                int year = dp != null ? dp.DateTime.Year : zeroDateTime.Year;
                int month = dp != null ? dp.DateTime.Month : zeroDateTime.Month;
                int day = dp != null ? dp.DateTime.Day : zeroDateTime.Day;
                int hour = tp != null ? tp.CurrentHour.IntValue() : zeroDateTime.Hour;
                int min = tp != null ? tp.CurrentMinute.IntValue() : zeroDateTime.Minute;

                result = new DateTime(year, month, day, hour, min, 0);
            }
            return result;
        }

        private Task<IDialogAnswer<object>> ShowSelectionDialog(string caption, KeyValuePair<object, string>[] items, int index, IDialogButton positive, IDialogButton negative)
        {
            var tcs = new TaskCompletionSource<IDialogAnswer<object>>();

            var builder = CreateBuilder();
            builder.SetTitle(caption);

            var rows = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
                rows[i] = items[i].Value;

            var adapt = new ArrayAdapter<string>(_activity, Android.Resource.Layout.SelectDialogSingleChoice, rows);
            builder.SetSingleChoiceItems(adapt, index, (e, args) => { });
            builder.SetPositiveButton(positive.Caption, (sender, e) =>
            {
                int pos = ((AlertDialog)sender).ListView.CheckedItemPosition;
                if (pos < items.Length)
                {
                    object result = items[pos].Key;
                    tcs.SetResult(new DialogAnswer<object>(true, result));
                }
            });
            builder.SetNegativeButton(negative.Caption, (sender, e) => tcs.SetResult(new DialogAnswer<object>(false, null)));
            builder.SetCancelable(false);

            ShowDialog(builder);

            return tcs.Task;
        }

        private void DisposeDialog(object sender, EventArgs e)
        {
            AlertDialog dialog = Dialogs.Pop();
            if (!ReferenceEquals(sender, dialog))
            {
                DisposeDialog(sender, e);
                Dialogs.Push(dialog);
            }
            else
                dialog.Dispose();
        }

        private AlertDialog.Builder CreateBuilder()
        {
            return new AlertDialog.Builder(_activity);
        }

        private void ShowDialog(AlertDialog.Builder builder)
        {
            _applicationContext.InvokeOnMainThread(() =>
            {
                using (builder)
                {
                    AlertDialog dialog = builder.Show();
                    dialog.DismissEvent += DisposeDialog;
                    Dialogs.Push(dialog);
                }
            });
        }

        private View CreateTabIndicator(string text)
        {
            var result = new TextView(_activity) { Text = text, Gravity = GravityFlags.Center, TextSize = 30 };
            return result;
        }

        public enum DialogButton
        {
            Positive,
            Negative,
            Neutral
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

        private class TabContentFactory : Java.Lang.Object, TabHost.ITabContentFactory
        {
            private readonly IDictionary<string, View> _content = new Dictionary<string, View>();

            public void Add(string tag, View view)
            {
                _content.Add(tag, view);
            }

            public View CreateTabContent(string tag)
            {
                return _content[tag];
            }

            protected override void Dispose(bool disposing)
            {
                foreach (var content in _content)
                    content.Value.Dispose();
                _content.Clear();

                base.Dispose(disposing);
            }
        }
    }
}