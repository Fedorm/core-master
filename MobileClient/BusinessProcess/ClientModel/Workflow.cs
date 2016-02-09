using System;
using System.Collections.Generic;
using BitMobile.Application;
using BitMobile.Application.Controls;
using BitMobile.Common.Application;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Workflow
    {
        private IApplicationContext Context
        {
            get
            {
                return ApplicationContext.Current;
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
            var dict = new Dictionary<string, object> { { "step", name } };

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
            var p = new Dictionary<string, object>();
            int i = 1;
            foreach (object obj in args)
            {
                p.Add("param" + i, obj);
                i++;
            }
            return p;
        }

        void OnExecute()
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
        }
    }
}

