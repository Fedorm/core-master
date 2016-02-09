using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using BitMobile.Common;
using BitMobile.Controls;
using BitMobile.Utilities.Translator;

namespace BitMobile.Droid.Providers
{
    class DialogProvider : IDialogProvider
    {
        public const int DatePicker = 1989;
        public const int TimePicker = 1411;

        private readonly BaseScreen _activity;
        private readonly ApplicationContext _context;

        public DialogProvider(BaseScreen activity, ApplicationContext context)
        {
            _activity = activity;
            _context = context;

            Dialogs = new Stack<AlertDialog>();
        }

        public Stack<AlertDialog> Dialogs { get; private set; }

        public void Alert(string message, DialogButton<int> positive, DialogButton<int> negative, DialogButton<int> neutral)
        {
            ShowDialog(null, message, positive, negative, neutral);
        }

        public void Message(string message, DialogButton<object> ok)
        {
            ShowDialog(null, message, ok);
        }

        public void Ask(string message, DialogButton<object> positive, DialogButton<object> negative)
        {
            ShowDialog(null, message, positive, negative);
        }

        public void DateTime(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative)
        {
            ShowDateTimeDialog(caption, current, true, true, positive, negative);
        }

        public void Date(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative)
        {
            ShowDateTimeDialog(caption, current, true, false, positive, negative);
        }

        public void Time(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative)
        {
            ShowDateTimeDialog(caption, current, false, true, positive, negative);
        }

        public void Choose(string caption, KeyValuePair<object, string>[] items, int index, DialogButton<object> positive, DialogButton<object> negative)
        {
            ShowSelectionDialog(caption, items, index, positive, negative);
        }

        internal void ShowDialog<T>(string caption, string message, DialogButton<T> positive, DialogButton<T> negative = null, DialogButton<T> neutral = null)
        {
            using (var builder = new AlertDialog.Builder(_activity))
            {
                builder.SetMessage(message);

                if (caption != null)
                    builder.SetTitle(caption);

                builder.SetPositiveButton(positive.Caption, (sender, e) => positive.Execute());

                if (negative != null)
                    builder.SetNegativeButton(negative.Caption, (sender, e) => negative.Execute());

                if (neutral != null)
                    builder.SetNeutralButton(neutral.Caption, (sender, e) => neutral.Execute());

                builder.SetCancelable(false);
                AlertDialog dialog = builder.Show();
                dialog.DismissEvent += DisposeDialog;
                Dialogs.Push(dialog);
            }
        }

        void ShowDateTimeDialog(string caption, DateTime current, bool date, bool time
            , DialogButton<DateTime> positive, DialogButton<DateTime> negative)
        {
            using (var builder = new AlertDialog.Builder(_activity))
            {
                builder.SetTitle(caption);
                using (var ll = new LinearLayout(_activity))
                {
                    ll.Orientation = Orientation.Vertical;
                    if (date)
                        using (var dtv = new Android.Widget.TextView(_activity))
                        using (var dp = new DatePicker(_activity))
                        {
                            dtv.Text = D.DATE;
                            dtv.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
                            dtv.Gravity = GravityFlags.Center;
                            ll.AddView(dtv);

                            dp.Id = DatePicker;
                            var minDate = new DateTime(1900, 1, 1);
                            dp.DateTime = current > minDate ? current : minDate;
                            ll.AddView(dp);
                        }
                    if (time)
                        using (var ttv = new Android.Widget.TextView(_activity))
                        using (var tp = new TimePicker(_activity))
                        {
                            ttv.Text = D.TIME;
                            ttv.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
                            ttv.Gravity = GravityFlags.Center;
                            ll.AddView(ttv);

                            tp.Id = TimePicker;
                            tp.SetIs24HourView(new Java.Lang.Boolean(true));
                            tp.CurrentHour = new Java.Lang.Integer(current.Hour);
                            tp.CurrentMinute = new Java.Lang.Integer(current.Minute);
                            ll.AddView(tp);

                        }
                    builder.SetView(ll);
                }

                builder.SetPositiveButton(positive.Caption, (sender, e) =>
                {
                    var alertDialog = (AlertDialog)sender;
                    DateTime result = DateTimeFromDialog(alertDialog);
                    positive.Execute(result);
                });

                builder.SetNegativeButton(negative.Caption, (sender, e) =>
                {
                    var alertDialog = (AlertDialog)sender;
                    DateTime result = DateTimeFromDialog(alertDialog);
                    negative.Execute(result);
                });

                builder.SetCancelable(false);
                AlertDialog dialog = builder.Show();
                dialog.DismissEvent += DisposeDialog;
                Dialogs.Push(dialog);
            }
        }

        static DateTime DateTimeFromDialog(AlertDialog alertDialog)
        {
            DateTime result;
            using (var dp = alertDialog.FindViewById<DatePicker>(DatePicker))
            using (var tp = alertDialog.FindViewById<TimePicker>(TimePicker))
            {
                if (dp != null)
                    dp.ClearFocus();
                if (tp != null)
                    tp.ClearFocus();

                int year = dp != null ? dp.DateTime.Year : 1;
                int month = dp != null ? dp.DateTime.Month : 1;
                int day = dp != null ? dp.DateTime.Day : 1;
                int hour = tp != null ? tp.CurrentHour.IntValue() : 0;
                int min = tp != null ? tp.CurrentMinute.IntValue() : 0;

                result = new DateTime(year, month, day, hour, min, 0);
            }
            return result;
        }

        void ShowSelectionDialog(string caption, KeyValuePair<object, string>[] items, int index, DialogButton<object> positive, DialogButton<object> negative)
        {
            using (var builder = new AlertDialog.Builder(_activity))
            {
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
                        positive.Execute(result);
                    }
                });

                builder.SetNegativeButton(negative.Caption, (sender, e) => negative.Execute());

                builder.SetCancelable(false);
                AlertDialog dialog = builder.Show();
                dialog.DismissEvent += DisposeDialog;
                Dialogs.Push(dialog);
            }
        }

        void DisposeDialog(object sender, EventArgs e)
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
    }
}