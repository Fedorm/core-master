using System;
using System.Collections.Generic;
using BitMobile.Common.Entites;

namespace BitMobile.Common.Log
{
    public interface ILogger
    {
        void ClearLog();
        void ScheduledClear();
        IEnumerable<ILog> GetLogs();
        void ScreenOpening(string screenName, string controllerName, IEnumerable<KeyValuePair<string, object>> parameters);
        void ScreenOpened();
        void SyncStarted();
        void SyncDownload(IEntityType type, bool isTombstone = false);
        void SyncUpload(IEntityType type, bool isTombstone = false);
        void SyncFinished();
        void ApplicationStarted();
        void ApplicationClosed();
        void ApplicationMinimized();
        void ApplicationMaximized();
        void WorkflowStarted(string name);
        void WorkflowPaused();
        void WorkflowFinished(string reason);
        void WorkflowForward(string nextStep, IEnumerable<KeyValuePair<string, object>> parameters);
        void WorkflowForwardNotAllowed(string nextStep, IEnumerable<KeyValuePair<string, object>> parameters);
        void WorkflowBack();
        void WorkflowBackTo(string step);
        void TextInput(string id, string text);
        void Clicked(string id, string action, string caption = "");
        void Error(string text, bool isFatal);
    }
}