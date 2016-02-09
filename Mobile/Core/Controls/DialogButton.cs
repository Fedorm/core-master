using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Controls
{
    public class DialogButton<TResult>
    {
        public DialogButton(string caption, Action<object, TResult> handler = null, object state = null)
        {
            Caption = caption;
            Handler = handler;
            State = state;
        }

        public string Caption { get; private set; }
        public Action<object, TResult> Handler { get; private set; }
        public object State { get; private set; }

        public void Execute()
        {
            if (Handler != null)
                Handler(State, default(TResult));
        }

        public void Execute(TResult result)
        {
            if (Handler != null)
                Handler(State, result);
        }
    }
}
