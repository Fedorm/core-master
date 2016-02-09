using System.Linq;
using BitMobile.Application;
using BitMobile.Controls;
using BitMobile.Script;
using BitMobile.Utilities.Translator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace BitMobile.ClientModel
{
    // ReSharper disable UnusedMember.Global, IntroduceOptionalParameters.Global, MemberCanBePrivate.Global
    public class Dialog
    {
        private const string NullString = "NULL";

        readonly ScriptEngine _scriptEngine;
        readonly IApplicationContext _context;

        public Dialog(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public void Alert(object message, IJSExecutable handler, object value, object positiveText)
        {
            Alert(message, handler, value, positiveText, null, null);
        }

        public void Alert(object message, IJSExecutable handler, object value, object positiveText, object negativeText)
        {
            Alert(message, handler, value, positiveText, negativeText, null);
        }

        public void Alert(object message, IJSExecutable handler, object value, object positiveText, object negativeText,
            object neutralText)
        {
            string msg = ObjToString(message);

            var positiveButtonText = ObjToString(positiveText);
            var positive = new DialogButton<int>(positiveButtonText, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<int>(0));
            }, value);

            DialogButton<int> negative = null;
            if (negativeText != null)
                negative = new DialogButton<int>(_context.DAL.TranslateString(negativeText.ToString()), (state, result) =>
                {
                    if (handler != null)
                        handler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<int>(1));
                }, value);

            DialogButton<int> neutral = null;
            if (neutralText != null)
                neutral = new DialogButton<int>(_context.DAL.TranslateString(neutralText.ToString()), (state, result) =>
                {
                    if (handler != null)
                        handler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<int>(2));
                }, value);

            _context.DialogProvider.Alert(msg, positive, negative, neutral);
        }

        public void Ask(object message, IJSExecutable positiveHandler)
        {
            Ask(message, positiveHandler, null, null, null);
        }

        public void Ask(object message, IJSExecutable positiveHandler, object positiveValue)
        {
            Ask(message, positiveHandler, positiveValue, null, null);
        }

        public void Ask(object message, IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler)
        {
            Ask(message, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Ask(object message
            , IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler, object negativeValue)
        {
            string msg = ObjToString(message);
            var yes = new DialogButton<object>(D.YES, (state, result) =>
            {
                if (positiveHandler != null)
                    positiveHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<Result>(Result.Yes));
            }, positiveValue);

            var no = new DialogButton<object>(D.NO, (state, result) =>
            {
                if (negativeHandler != null)
                    negativeHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<Result>(Result.No));
            }, negativeValue);

            _context.DialogProvider.Ask(msg, yes, no);
        }

        public void Message(object message)
        {
            Message(message, null, null);
        }

        public void Message(object message, IJSExecutable handler)
        {
            Message(message, handler, null);
        }

        public void Message(object message, IJSExecutable handler, object value)
        {
            string msg = ObjToString(message);

            var close = new DialogButton<object>(D.CLOSE, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<object>(null));
            }, value);

            _context.DialogProvider.Message(msg, close);
        }

        public void Debug(object variable)
        {
            string message = variable != null ? variable.ToString() : NullString;

            var close = new DialogButton<object>(D.CLOSE);

            _context.DialogProvider.Message(message, close);
        }

        public void DateTime(object caption, IJSExecutable positiveHandler)
        {
            DateTime(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void DateTime(object caption, IJSExecutable positiveHandler, object positiveValue)
        {
            DateTime(caption, System.DateTime.Now, positiveHandler, positiveValue, null, null);
        }

        public void DateTime(object caption, DateTime current, IJSExecutable positiveHandler)
        {
            DateTime(caption, current, positiveHandler, null, null, null);
        }

        public void DateTime(object caption, DateTime current, IJSExecutable positiveHandler, object positiveValue)
        {
            DateTime(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void DateTime(object caption, DateTime current, IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler)
        {
            DateTime(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void DateTime(object caption, DateTime current
            , IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler, object negativeValue)
        {
            DialogButton<DateTime> ok;
            DialogButton<DateTime> cancel;
            var capt = PrepareDateTime(caption, positiveHandler, positiveValue, negativeHandler, negativeValue, out ok, out cancel);

            _context.DialogProvider.DateTime(capt, current, ok, cancel);
        }

        public void Date(string caption, IJSExecutable positiveHandler)
        {
            Date(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void Date(string caption, IJSExecutable positiveHandler, object value)
        {
            Date(caption, System.DateTime.Now, positiveHandler, value, null, null);
        }

        public void Date(string caption, DateTime current, IJSExecutable positiveHandler)
        {
            Date(caption, current, positiveHandler, null, null, null);
        }

        public void Date(object caption, DateTime current, IJSExecutable positiveHandler, object positiveValue)
        {
            Date(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void Date(string caption, DateTime current, IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler)
        {
            Date(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Date(object caption, DateTime current
            , IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler, object negativeValue)
        {
            DialogButton<DateTime> ok;
            DialogButton<DateTime> cancel;
            var capt = PrepareDateTime(caption, positiveHandler, positiveValue, negativeHandler, negativeValue, out ok, out cancel);

            _context.DialogProvider.Date(capt, current, ok, cancel);
        }

        public void Time(string caption, IJSExecutable positiveHandler)
        {
            Time(caption, System.DateTime.Now, positiveHandler, null, null, null);
        }

        public void Time(string caption, IJSExecutable positiveHandler, object value)
        {
            Time(caption, System.DateTime.Now, positiveHandler, value, null, null);
        }

        public void Time(string caption, DateTime current, IJSExecutable positiveHandler)
        {
            Time(caption, current, positiveHandler, null, null, null);
        }

        public void Time(object caption, DateTime current, IJSExecutable positiveHandler, object positiveValue)
        {
            Time(caption, current, positiveHandler, positiveValue, null, null);
        }

        public void Time(string caption, DateTime current, IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler)
        {
            Time(caption, current, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Time(object caption, DateTime current
            , IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler, object negativeValue)
        {
            DialogButton<DateTime> ok;
            DialogButton<DateTime> cancel;
            var capt = PrepareDateTime(caption, positiveHandler, positiveValue, negativeHandler, negativeValue, out ok, out cancel);

            _context.DialogProvider.Time(capt, current, ok, cancel);
        }

        public void Choose(object caption, object items, IJSExecutable positiveHandler)
        {
            Choose(caption, items, null, positiveHandler, null, null, null);
        }

        public void Choose(object caption, object items, IJSExecutable positiveHandler, object positiveValue)
        {
            Choose(caption, items, null, positiveHandler, positiveValue, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJSExecutable positiveHandler)
        {
            Choose(caption, items, startKey, positiveHandler, null, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJSExecutable positiveHandler, object positiveValue)
        {
            Choose(caption, items, startKey, positiveHandler, positiveValue, null, null);
        }

        public void Choose(object caption, object items, object startKey, IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler)
        {
            Choose(caption, items, startKey, positiveHandler, positiveValue, negativeHandler, null);
        }

        public void Choose(object caption, object items, object startKey
            , IJSExecutable positiveHandler, object positiveValue, IJSExecutable negativeHandler, object negativeValue)
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

            var ok = new DialogButton<object>(D.OK, (state, result) =>
            {
                if (positiveHandler != null)
                    positiveHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<object>(result));
            }, positiveValue);

            var cancel = new DialogButton<object>(D.CANCEL, (state, result) =>
            {
                if (negativeHandler != null)
                    negativeHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<object>(result));
            }, negativeValue);

            _context.DialogProvider.Choose(capt, rows, index, ok, cancel);
        }

        #region Obsolete

        public void Question(string message, IJSExecutable handler)
        {
            Question(message, handler, null);
        }

        public void Question(string message, IJSExecutable handler, object value)
        {
            message = _context.DAL.TranslateString(message);

            var yes = new DialogButton<object>(D.YES, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteStandalone(_scriptEngine.Visitor, Result.Yes, state);
            }, value);

            var no = new DialogButton<object>(D.NO, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteStandalone(_scriptEngine.Visitor, Result.No, state);
            }, value);

            _context.DialogProvider.Ask(message, yes, no);
        }

        public void ShowDateTime(string caption, IJSExecutable handler)
        {
            ShowDateTime(caption, System.DateTime.Now, handler, null);
        }

        public void ShowDateTime(string caption, IJSExecutable handler, object value)
        {
            ShowDateTime(caption, System.DateTime.Now, handler, value);
        }

        public void ShowDateTime(string caption, DateTime current, IJSExecutable handler)
        {
            ShowDateTime(caption, current, handler, null);
        }

        public void ShowDateTime(string caption, DateTime current, IJSExecutable handler, object value)
        {
            caption = _context.DAL.TranslateString(caption);

            var ok = new DialogButton<DateTime>(D.OK, (state, result) =>
                {
                    if (handler != null)
                        handler.ExecuteStandalone(_scriptEngine.Visitor, result, state);
                }, value);

            var cancel = new DialogButton<DateTime>(D.CANCEL);

            _context.DialogProvider.DateTime(caption, current, ok, cancel);
        }

        public void ShowDate(string caption, IJSExecutable handler)
        {
            ShowDate(caption, System.DateTime.Now, handler, null);
        }

        public void ShowDate(string caption, IJSExecutable handler, object value)
        {
            ShowDate(caption, System.DateTime.Now, handler, value);
        }

        public void ShowDate(string caption, DateTime current, IJSExecutable handler)
        {
            ShowDate(caption, current, handler, null);
        }

        public void ShowDate(string caption, DateTime current, IJSExecutable handler, object value)
        {
            caption = _context.DAL.TranslateString(caption);

            var ok = new DialogButton<DateTime>(D.OK, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteStandalone(_scriptEngine.Visitor, result, state);
            }, value);

            var cancel = new DialogButton<DateTime>(D.CANCEL);

            _context.DialogProvider.Date(caption, current, ok, cancel);
        }

        public void ShowTime(string caption, IJSExecutable handler)
        {
            ShowTime(caption, System.DateTime.Now, handler, null);
        }

        public void ShowTime(string caption, IJSExecutable handler, object value)
        {
            ShowTime(caption, System.DateTime.Now, handler, value);
        }

        public void ShowTime(string caption, DateTime current, IJSExecutable handler)
        {
            ShowTime(caption, current, handler, null);
        }

        public void ShowTime(string caption, DateTime current, IJSExecutable handler, object value)
        {
            caption = _context.DAL.TranslateString(caption);

            var ok = new DialogButton<DateTime>(D.OK, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteStandalone(_scriptEngine.Visitor, result, state);
            }, value);

            var cancel = new DialogButton<DateTime>(D.CANCEL);

            _context.DialogProvider.Time(caption, current, ok, cancel);
        }

        public void Select(string caption, object items, IJSExecutable handler)
        {
            Select(caption, items, handler, null);
        }

        public void Select(string caption, object items, IJSExecutable handler, object value)
        {
            KeyValuePair<object, string>[] rows = PrepareSelection(items);

            caption = _context.DAL.TranslateString(caption);

            var ok = new DialogButton<object>(D.OK, (state, result) =>
            {
                if (handler != null)
                    handler.ExecuteStandalone(_scriptEngine.Visitor, result, state);
            }, value);

            var cancel = new DialogButton<object>(D.CANCEL);

            _context.DialogProvider.Choose(caption, rows, 0, ok, cancel);
        }
        #endregion

        private string ObjToString(object obj)
        {
            string text = obj != null ? obj.ToString() : NullString;
            text = _context.DAL.TranslateString(text);
            return text;
        }

        private string PrepareDateTime(object caption, IJSExecutable positiveHandler, object positiveValue,
            IJSExecutable negativeHandler, object negativeValue, out DialogButton<DateTime> ok, out DialogButton<DateTime> cancel)
        {
            string capt = ObjToString(caption);

            ok = new DialogButton<DateTime>(D.OK, (state, result) =>
            {
                if (positiveHandler != null)
                    positiveHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<DateTime>(result));
            }, positiveValue);

            cancel = new DialogButton<DateTime>(D.CANCEL, (state, result) =>
            {
                if (negativeHandler != null)
                    negativeHandler.ExecuteCallback(_scriptEngine.Visitor, state, new DialogArgs<DateTime>(result));
            }, negativeValue);
            return capt;
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
                        object key = subarray[0];
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

        // ReSharper disable MemberHidesStaticFromOuterClass, UnusedAutoPropertyAccessor.Global
        public class DialogArgs<TResult>
        {
            public DialogArgs(TResult result)
            {
                Result = result;
            }

            public TResult Result { get; private set; }
        }

        public enum Result
        {
            Yes,
            No
        }
    }
}