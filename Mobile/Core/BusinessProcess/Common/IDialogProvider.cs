using System;
using System.Collections.Generic;
using BitMobile.Controls;

namespace BitMobile.Common
{
    public interface IDialogProvider
    {
        void Alert(string message, DialogButton<int> positive, DialogButton<int> negative
            , DialogButton<int> neutral);

        void Message(string message, DialogButton<object> ok);

        void Ask(string message, DialogButton<object> positive, DialogButton<object> negative);

        void DateTime(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative);

        void Date(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative);

        void Time(string caption, DateTime current, DialogButton<DateTime> positive, DialogButton<DateTime> negative);

        void Choose(string caption, KeyValuePair<object, string>[] items, int index
            , DialogButton<object> positive, DialogButton<object> negative);
    }
}