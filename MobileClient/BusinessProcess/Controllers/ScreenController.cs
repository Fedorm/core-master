using System;
using System.Collections.Concurrent;
using BitMobile.Application;
using BitMobile.Common.BusinessProcess.Controllers;

namespace BitMobile.BusinessProcess.Controllers
{
    public class ScreenController : Controller, IScreenController
    {
        public static ScreenController Current { get; private set; }

        readonly ConcurrentQueue<string> _pushMessages = new ConcurrentQueue<string>();

        public ScreenController()
        {
            ApplicationContext.Current.ApplicationRestore += CurrentOnApplicationRestore;
        }

        public String GetView(String defaultView)
        {
            return defaultView;
        }

        public void OnLoading(string screenName)
        {
            CallFunctionNoException("OnLoading", new object[] { screenName });
        }

        public void OnLoad(string screenName)
        {
            CallFunctionNoException("OnLoad", new object[] { screenName });
        }

        public void SetCurrentScreenController()
        {
            Current = this;
        }

        internal void OnPushMessage(string message)
        {
            if (ApplicationContext.Current.InBackground)
                _pushMessages.Enqueue(message);
            else
                CallOnPushMessage(message);
        }

        private void CurrentOnApplicationRestore()
        {
            while (!_pushMessages.IsEmpty)
            {
                string message;
                while (_pushMessages.TryDequeue(out message))
                    CallOnPushMessage(message);
            }
        }

        private void CallOnPushMessage(string message)
        {
            ApplicationContext.Current.InvokeOnMainThread(() =>
                CallFunctionNoException("OnPushMessage", new object[] { message }));
        }
    }
}
