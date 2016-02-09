using System;
using System.Collections.Generic;
using System.Reflection;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.Log;

namespace BitMobile.Application.Log
{
    public abstract class CustomExceptionHandler : IExceptionHandler
    {
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

        public IReport GetReport(bool isFatal, string text)
        {
            IReport report = LogManager.Reporter.CreateReport(text, isFatal ? ReportType.Crash : ReportType.Warning);
            report.MakeAttachment(GetNativeInfo());
            return report;
        }

        void HandlePrepared(Exception e)
        {
            if (e.Message.Contains("database disk image is malformed"))
                HandleException(true, true, D.WARNING, D.DATABASE_IS_MALFORMED, e.ToString());
            else
            {
                var ce = e as CustomException;
                if (ce != null)
                    HandleException(ce.IsFatal, ce.IsFatal, D.WARNING, ce.FriendlyMessage, ce.Report);
                else
                    HandleException(true, true, D.WARNING, D.UNEXPECTED_ERROR_OCCURED, e.ToString());
            }
        }

        void HandleException(bool shutDownAfterHandle, bool isFatal, string title, string message, object report, Action next = null)
        {
            string cancelButtonTitle = D.OK;
            string[] otherButtons = null;

            if (next == null)
                next = () => { };
            Action nextColsed = next;
            Action<int> onClick = index => nextColsed();

            if (shutDownAfterHandle)
            {
                message += Environment.NewLine + D.APPLICATION_WILL_BE_CANCELLED;
                next = ShutDownApplication;
            }

            if (report != null)
            {
                if (LogManager.Reporter != null)
                {
                    IReport r = report as IReport ?? GetReport(false, report.ToString());
                    LogManager.Logger.Error(r.Text, isFatal);
                }

                message += Environment.NewLine + D.SEND_ERROR;
                cancelButtonTitle = D.YES;
                otherButtons = new[] { D.NO };
                onClick = index => OnClick(isFatal, report, next, index);
            }

            if (isFatal)
                ShowDialog(title, message, onClick, cancelButtonTitle, otherButtons);
            else
                next();
        }

        protected abstract IDictionary<string, string> GetNativeInfo();
        
        protected abstract void ShutDownApplication();

        protected abstract void ShowDialog(string title, string message, Action<int> onClick, string cancelButtonTitle, params string[] otherButtons);

        protected abstract void PrepareException(Exception e, Action<Exception> next);

        protected static string PropertiesToString(object obj)
        {
            string result = "";
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = property.GetValue(obj);
                result += string.Format("<{0}>{1}</{0}>\r\n", property.Name, value ?? "null");
            }
            return result;
        }

        private async void OnClick(bool isFatal, object report, Action next, int index)
        {
            if (index == 0)
            {
                var r = report as IReport ?? GetReport(isFatal, report.ToString());

                if (await LogManager.Reporter.Send(r))
                    next();
                else
                    ShowDialog(D.WARNING, D.CANNOT_SEND_ERROR, idx => OnClick(isFatal, report, next, idx), D.YES, D.NO);
            }
            else
                next();
        }
    }
}

