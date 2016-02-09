using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using BitMobile.Application.TestsAgent;
using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;
using BitMobile.Droid.Controls;
using BitMobile.Droid.Providers;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using BitMobile.ValueStack;

namespace BitMobile.Droid.Tests
{
    class AndroidViewProxy : ViewProxy
    {
        // ReSharper disable once NotAccessedField.Local
        BaseScreen _activity;

        public AndroidViewProxy(Application.AndroidApplicationContext context, BaseScreen activity)
            : base(context)
        {
            _activity = activity;
        }

        Application.AndroidApplicationContext Context
        {
            get { return (Application.AndroidApplicationContext)_context; }
        }

        public override bool DoClick(object obj)
        {
            var control = obj as IControl<View>;
            if (control != null && control.View != null)
            {
                _context.InvokeOnMainThreadSync(() =>
                    {
                        control.View.DispatchTouchEvent(MotionEvent.Obtain(SystemClock.UptimeMillis()
                            , SystemClock.UptimeMillis() + 100, MotionEventActions.Down, 0, 0, 0));

                        Thread.Sleep(100);

                        control.View.DispatchTouchEvent(MotionEvent.Obtain(SystemClock.UptimeMillis()
                            , SystemClock.UptimeMillis() + 100, MotionEventActions.Up, 0, 0, 0));
                    });

                return true;
            }
            return false;
        }

        public override bool DoSetFocus(object obj)
        {
            var control = obj as IControl<View>;
            if (control != null)
            {
                View view = control.View;
                _context.InvokeOnMainThread(() =>
                {
                    if (view == null)
                    {
                        control.CreateView();
                        view = control.View;
                    }
                    view.RequestFocus();
                });
                return true;
            }

            return false;
        }

        public override bool DoSetText(object obj, string text)
        {
            var control = obj as CustomEdit;

            if (control != null)
            {
                var edit = (CustomEdit.EditTextNative)control.View;
                if (edit == null)
                    _context.InvokeOnMainThreadSync(() =>
                    {
                        control.CreateView();
                        edit = (CustomEdit.EditTextNative) control.View;
                    });

                DoSetFocus(control);

                string input = string.Empty;
                foreach (char chr in text)
                {
                    input += chr;
                    string txt = input;
                    _context.InvokeOnMainThread(() => edit.SetText(txt, Android.Widget.TextView.BufferType.Normal));
                    Thread.Sleep(300);
                }
                return true;
            }
            return false;
        }

        public override bool DoSetValue(object obj, string property, object value)
        {
            property = property.Trim();
            var package = obj as IEntity;
            if (package != null)
            {
                if (package.HasProperty(property))
                {
                    package.SetValue(property, value);
                    return true;
                }
                return false;
            }
            PropertyInfo pi = obj.GetType()
                .GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pi != null)
            {
                object param = Convert.ChangeType(value, pi.PropertyType);
                _context.InvokeOnMainThreadSync(() => pi.SetValue(obj, param));
                return true;
            }
            throw new Exception(string.Format("Cannot find property {0} in {1}", property, obj.GetType()));
        }

        public override bool DoScrollTo(object obj, string index)
        {
            int idx = int.Parse(index);

            var control = obj as ScrollView;
            if (control != null && control.View != null)
            {
                var controls = control.Controls;
                if (idx >= controls.Length)
                    idx = controls.Length - 1;

                _context.InvokeOnMainThreadSync(() => ((Android.Widget.ListView)control.View).SmoothScrollToPosition(idx));

                return true;
            }
            return false;
        }

        public override Stream DoTakeScreenshot()
        {
            View rootView = ((IControl<View>)_context.CurrentScreen.Screen).View.RootView;
            rootView.DrawingCacheEnabled = true;
            Bitmap bitmap = null;
            _context.InvokeOnMainThreadSync(() => bitmap = rootView.DrawingCache);

            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            return stream;
        }

        public override bool DoDialogClickPositive()
        {
            bool result = false;
            _context.InvokeOnMainThreadSync(() =>
            {
                AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
                if (dialog != null)
                    using (var button = dialog.GetButton((int)DialogButtonType.Positive))
                        result = button.PerformClick();
            });
            return result;
        }

        public override bool DoDialogClickNegative()
        {
            bool result = false;
            _context.InvokeOnMainThreadSync(() =>
            {
                AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
                if (dialog != null)
                    using (var button = dialog.GetButton((int)DialogButtonType.Negative))
                        result = button.PerformClick();
            });
            return result;
        }

        public override string DoDialogGetMessage()
        {
            string result = "";
            AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
            if (dialog != null)
                using (var tv = dialog.FindViewById<Android.Widget.TextView>(Android.Resource.Id.Message))
                    result = tv.Text;

            return result;
        }

        public override string DoDialogGetDateTime()
        {
            string result = "";
            AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
            if (dialog != null)
            {
                var dp = dialog.FindViewById<Android.Widget.DatePicker>(DialogProvider.DatePicker);
                var tp = dialog.FindViewById<Android.Widget.TimePicker>(DialogProvider.TimePicker);

                var dateTime = new DateTime(dp.DateTime.Year
                                , dp.DateTime.Month
                                , dp.DateTime.Day
                                , tp.CurrentHour.IntValue()
                                , tp.CurrentMinute.IntValue()
                                , 0);

                result = dateTime.ToString(CultureInfo.InvariantCulture);
            }

            return result;
        }

        public override bool DoDialogSetDateTime(object hack, string value)
        {
            DateTime dateTime = DateTime.Parse(value);

            bool result = false;
            _context.InvokeOnMainThreadSync(() =>
            {
                AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
                if (dialog != null)
                {
                    var dp = dialog.FindViewById<Android.Widget.DatePicker>(DialogProvider.DatePicker);
                    var tp = dialog.FindViewById<Android.Widget.TimePicker>(DialogProvider.TimePicker);

                    dp.UpdateDate(dateTime.Year, dateTime.Month, dateTime.Day);
                    tp.CurrentHour = new Java.Lang.Integer(dateTime.Hour);
                    tp.CurrentMinute = new Java.Lang.Integer(dateTime.Minute);

                    result = true;
                }
            });

            return result;
        }

        public override bool DoDialogSelectItem(object hack, string value)
        {
            Exception throwed = null;

            int index = int.Parse(value);

            bool result = false;
            _context.InvokeOnMainThreadSync(() =>
            {
                try
                {
                    AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
                    if (dialog != null)
                    {
                        if (dialog.ListView.Count <= index)
                            throw new Exception("Index out of bounds");

                        dialog.ListView.SetItemChecked(index, true);
                        result = true;
                    }
                }
                catch (Exception e)
                {
                    throwed = e;
                }
            });

            if (throwed != null)
                throw throwed;

            return result;
        }

        public override string DoDialogGetItem(object hack, string value)
        {
            Exception throwed = null;

            int index = int.Parse(value);

            string result = "";
            _context.InvokeOnMainThreadSync(() =>
            {
                try
                {
                    AlertDialog dialog = Context.DialogProviderInternal.Dialogs.Peek();
                    if (dialog != null)
                    {
                        if (dialog.ListView.Count <= index)
                            throw new Exception("Index out of bounds");

                        var obj = dialog.ListView.GetItemAtPosition(index);
                        result = obj.ToString();
                    }
                }
                catch (Exception e)
                {
                    throwed = e;
                }
            });

            if (throwed != null)
                throw throwed;

            return result;
        }
    }
}