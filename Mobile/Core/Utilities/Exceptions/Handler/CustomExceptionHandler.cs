using System;
using BitMobile.Utilities.Translator;
using BitMobile.Utilities.LogManager;
using System.Collections.Generic;

namespace BitMobile.Utilities.Exceptions
{
    public abstract class CustomExceptionHandler
    {
        protected const string EMAIL_TITLE = "BitMobile report";

        public void Handle(Exception e)
        {
            PrepareException(e, HandlePrepared);
        }

        public void HandleNonFatal(Exception e)
        {
            var newException = new NonFatalException(D.UNEXPECTED_ERROR_OCCURED, e.Message, e);
            PrepareException(newException, HandlePrepared);
        }

        public void Handle(object report, Action next)
        {
            HandleException(false, true, D.WARNING, D.APP_HAS_BEEN_INTERRUPTED, report, next);
        }

        public Log GetLog(bool isFatal, string report)
        {
            Log log = new Log();
            log.Text = report;
            log.IsCrash = isFatal;
            PrepareLog(log);
            return log;
        }

        void HandlePrepared(Exception e)
        {
            CustomException ce = e as CustomException;
            if (ce != null)
                HandleException(ce.IsFatal, ce.IsFatal, D.WARNING, ce.FriendlyMessage, ce.Report);
            else
                HandleException(true, true, D.WARNING, D.UNEXPECTED_ERROR_OCCURED, e.ToString());
        }

        void HandleException(bool shutDownAfterHandle, bool isFatal, string title, string message, object report, Action next = null)
        {
            string cancelButtonTitle = D.OK;
            string[] otherButtons = null;

            if (next == null)
                next = () =>
                {
                };
            Action<int> onClick = (index) => next();

            if (shutDownAfterHandle)
            {
                message += Environment.NewLine + D.APPLICATION_WILL_BE_CANCELLED;
                next = ShutDownApplication;
            }

            if (report != null)
            {
                message += Environment.NewLine + D.SEND_ERROR;
                cancelButtonTitle = D.YES;
                otherButtons = new string[] {
					D.NO
				};
                onClick = (index) => OnClick(isFatal, report, next, index);
            }
            ShowDialog(title, message, onClick, cancelButtonTitle, otherButtons);
        }

        protected abstract void SendLog(Log log, Action onSuccess, Action onFail);

        protected abstract void PrepareLog(Log log);

        protected abstract void ShutDownApplication();

        protected abstract void ShowDialog(string title, string message, Action<int> onClick, string cancelButtonTitle, params string[] otherButtons);

        protected abstract void PrepareException(Exception e, Action<Exception> next);

        private void OnClick(bool isFatal, object report, Action next, int index)
        {
            if (index == 0)
            {
                Log log;
                if (report is Log)
                    log = (Log)report;
                else
                    log = GetLog(isFatal, report.ToString());

                SendLog(log, next, () =>
                    ShowDialog(D.WARNING, D.CANNOT_SEND_ERROR, idx => OnClick(isFatal, report, next, idx), D.YES, D.NO));
            }
            else
                next();
        }
    }
}

