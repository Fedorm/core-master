using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BitMobile.Application.Controls;
using BitMobile.Application.Translator;
using BitMobile.Common.Application;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Dialog
    {
        public const string NullChooseKey = "@@@###NULL###@@@";
        private const string NullString = "NULL";

        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _context;

        public Dialog(IScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public void Alert(object message, IJsExecutable handler, object value, object positiveText)
        {
            Alert(message, handler, value, positiveText, null, null);
        }

        public void Alert(object message, IJsExecutable handler, object value, object positiveText, object negativeText)
        {
            Alert(message, handler, value, positiveText, negativeText, null);
        }

        public async void Alert(object message, IJsExecutable handler, object value, object positiveText, object negativeText,
            object neutralText)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            try
            {
                string msg = ObjToString(message);

                var positiveButtonText = ObjToString(positiveText);
                var positive = new DialogButton(positiveButtonText);

                DialogButton negative = null;
                if (negativeText != null)
                    negative = new DialogButton(_context.Dal.TranslateString(negativeText.ToString()));

                DialogButton neutral = null;
                if (neutralText != null)
                    neutral = new DialogButton(_context.Dal.TranslateString(neutralText.ToString()));

                int number = await _context.DialogProvider.Alert(msg, positive, negative, neutral);
                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, value, new Args<int>(number));
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        public void Ask(object message, IJsExecutable positiveHandler)
        {
            Ask(message, positiveHandler, null, null, null);
        }

        public void Ask(object message, IJsExecutable positiveHandler, object positiveValue)
        {
            Ask(message, positiveHandler, positiveValue, null, null);
        }

        public void Ask(object message, IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler)
        {
            Ask(message, positiveHandler, positiveValue, negativeHandler, null);
        }

        public async void Ask(object message
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();

            try
            {
                string msg = ObjToString(message);

                var yes = new DialogButton(D.YES);
                var no = new DialogButton(D.NO);

                bool positive = await _context.DialogProvider.Ask(msg, yes, no);

                IJsExecutable handler = positive ? positiveHandler : negativeHandler;
                object value = positive ? positiveValue : negativeValue;

                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, value, new Args<Result>(positive ? Result.Yes : Result.No));
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        public void Message(object message)
        {
            Message(message, null, null);
        }

        public void Message(object message, IJsExecutable handler)
        {
            Message(message, handler, null);
        }

        public async void Message(object message, IJsExecutable handler, object value)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            try
            {
                string msg = ObjToString(message);

                var close = new DialogButton(D.CLOSE);

                await _context.DialogProvider.Message(msg, close);

                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, value, new Args<object>(null));
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        public async void Debug(object variable)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            try
            {
                string message = variable != null ? variable.ToString() : NullString;

                var close = new DialogButton(D.CLOSE);

                await _context.DialogProvider.Message(message, close);
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        public void DateTime(object caption, IJsExecutable positiveHandler)
        {
            DateTime(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void DateTime(object caption, IJsExecutable positiveHandler, object positiveValue)
        {
            DateTime(caption, System.DateTime.Now, positiveHandler, positiveValue, null, null);
        }

        public void DateTime(object caption, DateTime current, IJsExecutable positiveHandler)
        {
            DateTime(caption, current, positiveHandler, null, null, null);
        }

        public void DateTime(object caption, DateTime current, IJsExecutable positiveHandler, object positiveValue)
        {
            DateTime(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void DateTime(object caption, DateTime current, IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler)
        {
            DateTime(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void DateTime(object caption, DateTime current
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            AnyDateTimeDialog(_context.DialogProvider.DateTime, caption, current
                , positiveHandler, positiveValue, negativeHandler, negativeValue);
        }

        public void Date(string caption, IJsExecutable positiveHandler)
        {
            Date(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void Date(string caption, IJsExecutable positiveHandler, object value)
        {
            Date(caption, System.DateTime.Now, positiveHandler, value, null, null);
        }

        public void Date(string caption, DateTime current, IJsExecutable positiveHandler)
        {
            Date(caption, current, positiveHandler, null, null, null);
        }

        public void Date(object caption, DateTime current, IJsExecutable positiveHandler, object positiveValue)
        {
            Date(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void Date(string caption, DateTime current, IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler)
        {
            Date(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Date(object caption, DateTime current
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            AnyDateTimeDialog(_context.DialogProvider.Date, caption, current
                , positiveHandler, positiveValue, negativeHandler, negativeValue);
        }

        public void Time(string caption, IJsExecutable positiveHandler)
        {
            Time(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void Time(string caption, IJsExecutable positiveHandler, object value)
        {
            Time(caption, System.DateTime.Now, positiveHandler, value, null, null);
        }

        public void Time(string caption, DateTime current, IJsExecutable positiveHandler)
        {
            Time(caption, current, positiveHandler, null, null, null);
        }

        public void Time(object caption, DateTime current, IJsExecutable positiveHandler, object positiveValue)
        {
            Time(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void Time(string caption, DateTime current, IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler)
        {
            Time(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Time(object caption, DateTime current
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            AnyDateTimeDialog(_context.DialogProvider.Time, caption, current
                , positiveHandler, positiveValue, negativeHandler, negativeValue);
        }

        public void Choose(object caption, object items, IJsExecutable positiveHandler)
        {
            Choose(caption, items, null, positiveHandler, null, null, null);
        }

        public void Choose(object caption, object items, IJsExecutable positiveHandler, object positiveValue)
        {
            Choose(caption, items, null, positiveHandler, positiveValue, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJsExecutable positiveHandler)
        {
            Choose(caption, items, startKey, positiveHandler, null, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJsExecutable positiveHandler, object positiveValue)
        {
            Choose(caption, items, startKey, positiveHandler, positiveValue, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler)
        {
            Choose(caption, items, startKey, positiveHandler, positiveValue, negativeHandler, null);
        }

        public async void Choose(object caption, object items, object startKey
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            try
            {
                KeyValuePair<object, string>[] rows = PrepareSelection(items);
                int index = 0;
                for (int i = 0; i < rows.Length; i++)
                    if (rows[i].Key.Equals(startKey))
                    {
                        index = i;
                        break;
                    }

                string capt = ObjToString(caption);

                var ok = new DialogButton(D.OK);
                var cancel = new DialogButton(D.CANCEL);

                IDialogAnswer<object> answer = await _context.DialogProvider.Choose(capt, rows, index, ok, cancel);

                IJsExecutable handler = answer.Positive ? positiveHandler : negativeHandler;
                object value = answer.Positive ? positiveValue : negativeValue;

                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, value, new KeyValueArgs<object, string>(answer.Result, rows));
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        #region Obsolete

        public void Question(string message, IJsExecutable handler)
        {
            Question(message, handler, null);
        }

        public async void Question(string message, IJsExecutable handler, object value)
        {
            message = _context.Dal.TranslateString(message);

            var yes = new DialogButton(D.YES);
            var no = new DialogButton(D.NO);

            bool positive = await _context.DialogProvider.Ask(message, yes, no);

            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, positive ? Result.Yes : Result.No, value);
        }

        public void ShowDateTime(string caption, IJsExecutable handler)
        {
            ShowDateTime(caption, System.DateTime.Now, handler, null);
        }

        public void ShowDateTime(string caption, IJsExecutable handler, object value)
        {
            ShowDateTime(caption, System.DateTime.Now, handler, value);
        }

        public void ShowDateTime(string caption, DateTime current, IJsExecutable handler)
        {
            ShowDateTime(caption, current, handler, null);
        }

        public async void ShowDateTime(string caption, DateTime current, IJsExecutable handler, object value)
        {
            caption = _context.Dal.TranslateString(caption);

            var ok = new DialogButton(D.OK);
            var cancel = new DialogButton(D.CANCEL);

            IDialogAnswer<DateTime> answer = await _context.DialogProvider.DateTime(caption, current, ok, cancel);

            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, answer.Result, value);
        }

        public void ShowDate(string caption, IJsExecutable handler)
        {
            ShowDate(caption, System.DateTime.Now, handler, null);
        }

        public void ShowDate(string caption, IJsExecutable handler, object value)
        {
            ShowDate(caption, System.DateTime.Now, handler, value);
        }

        public void ShowDate(string caption, DateTime current, IJsExecutable handler)
        {
            ShowDate(caption, current, handler, null);
        }

        public async void ShowDate(string caption, DateTime current, IJsExecutable handler, object value)
        {
            caption = _context.Dal.TranslateString(caption);

            var ok = new DialogButton(D.OK);
            var cancel = new DialogButton(D.CANCEL);

            IDialogAnswer<DateTime> answer = await _context.DialogProvider.Date(caption, current, ok, cancel);

            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, answer.Result, value);
        }

        public void ShowTime(string caption, IJsExecutable handler)
        {
            ShowTime(caption, System.DateTime.Now, handler, null);
        }

        public void ShowTime(string caption, IJsExecutable handler, object value)
        {
            ShowTime(caption, System.DateTime.Now, handler, value);
        }

        public void ShowTime(string caption, DateTime current, IJsExecutable handler)
        {
            ShowTime(caption, current, handler, null);
        }

        public async void ShowTime(string caption, DateTime current, IJsExecutable handler, object value)
        {
            caption = _context.Dal.TranslateString(caption);

            var ok = new DialogButton(D.OK);
            var cancel = new DialogButton(D.CANCEL);

            IDialogAnswer<DateTime> answer = await _context.DialogProvider.Time(caption, current, ok, cancel);

            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, answer.Result, value);
        }

        public void Select(string caption, object items, IJsExecutable handler)
        {
            Select(caption, items, handler, null);
        }

        public async void Select(string caption, object items, IJsExecutable handler, object value)
        {
            KeyValuePair<object, string>[] rows = PrepareSelection(items);

            caption = _context.Dal.TranslateString(caption);

            var ok = new DialogButton(D.OK);

            var cancel = new DialogButton(D.CANCEL);

            IDialogAnswer<object> answer = await _context.DialogProvider.Choose(caption, rows, 0, ok, cancel);

            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, answer.Result, value);
        }
        #endregion

        private string ObjToString(object obj)
        {
            string text = obj != null ? obj.ToString() : NullString;
            text = _context.Dal.TranslateString(text);
            return text;
        }

        private async void AnyDateTimeDialog(DateTimeDialog func, object caption, DateTime current
            , IJsExecutable positiveHandler, object positiveValue, IJsExecutable negativeHandler, object negativeValue)
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            try
            {
                string capt = ObjToString(caption);

                var ok = new DialogButton(D.OK);
                var cancel = new DialogButton(D.CANCEL);

                IDialogAnswer<DateTime> answer = await func(capt, current, ok, cancel);

                IJsExecutable handler = answer.Positive ? positiveHandler : negativeHandler;
                object value = answer.Positive ? positiveValue : negativeValue;

                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, value, new Args<DateTime>(answer.Result));
            }
            finally
            {
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
        static KeyValuePair<object, string>[] PrepareSelection(object items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            var rows = new List<KeyValuePair<object, string>>();

            if (items is ArrayList)
            {
                var array = (ArrayList)items;
                foreach (ArrayList subarray in array)
                {
                    if (subarray != null && subarray.Count == 2)
                    {
                        object key = subarray[0] ?? NullChooseKey;
                        string str = subarray[1] != null ? subarray[1].ToString() : string.Empty;
                        rows.Add(new KeyValuePair<object, string>(key, str));
                    }
                    else
                        throw new ArgumentException("Dialog.Selector: Items has invalid subarray");
                }
            }
            else if (items is IDataReader)
            {
                var reader = (IDataReader)items;
                while (reader.Read())
                {
                    if (reader.FieldCount == 2)
                    {
                        object key = reader.GetValue(0);
                        string str = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        rows.Add(new KeyValuePair<object, string>(key, str));
                    }
                    else
                        throw new ArgumentException("Dialog.Selector: Items has invalid data record");
                }
            }
            return rows.ToArray();
        }
        // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull

        private delegate Task<IDialogAnswer<DateTime>> DateTimeDialog(
            string caption, DateTime current, IDialogButton positive, IDialogButton negative);

        public class DialogButton : IDialogButton
        {
            public DialogButton(string caption)
            {
                Caption = caption;
            }

            public string Caption { get; private set; }
        }

        public enum Result
        {
            Yes,
            No
        }
    }
}