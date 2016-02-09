using System;
using System.Collections.Generic;
using System.Linq;

using BitMobile.ValueStack;
using BitMobile.Controls;

namespace BitMobile.ClientModel
{
    public class Workflow
    {
        private BitMobile.Application.IApplicationContext Context
        {
            get
            {
                return BitMobile.Application.ApplicationContext.Context;
            }
        }

        public void Forward(System.Collections.ArrayList args)
        {
            OnExecute();
            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, "Forward", DictionaryFromArray(args)));
        }

        public void Back()
        {
            OnExecute();
            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, "Back", null));
        }

        public void BackTo(String name)
        {
            OnExecute();
            Dictionary<String, object> dict = new Dictionary<string, object>();
            dict.Add("step", name);

            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, "BackTo", dict, true));
        }

        public void Commit()
        {
            OnExecute();
            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, "Commit", null));
        }

        public void Rollback()
        {
            OnExecute();
            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, "Rollback", null));
        }

        public void Action(String name, System.Collections.ArrayList args)
        {
            OnExecute();
            Context.InvokeOnMainThread(() => Context.Workflow.InvokeAction(Context, name, DictionaryFromArray(args)));
        }

        public void Refresh(System.Collections.ArrayList args)
        {
            OnExecute();
			Context.InvokeOnMainThread(() => Context.Workflow.Refresh(Context, DictionaryFromArray(args)));
        }

        Dictionary<String, object> DictionaryFromArray(System.Collections.ArrayList args)
        {
            Dictionary<String, object> p = new Dictionary<string, object>();
            int i = 1;
            foreach (object obj in args)
            {
                p.Add("param" + i.ToString(), obj);
                i++;
            }
            return p;
        }

        void OnExecute()
        {
            ActionHandler.Busy = true;
            ActionHandlerEx.Busy = true;
        }
    }
}

