using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Application.DbEngine;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.Log;
using BitMobile.Application;
using System.Threading;

namespace BitMobile.Log
{
    public class Logger : ILogger
    {
        private const string IdIsEmpty = "id is empty";
        private const int DefaultLogMinCount = 10000;
        private readonly TimeSpan _defaultLogLifetime = new TimeSpan(7, 0, 0, 0);

        private readonly TimeSpan _logLifetime;
        private readonly int _logMinCount;
        private bool _enabled;
        private Dictionary<IEntityType, int> _syncDownloadLog;
        private Dictionary<IEntityType, int> _syncDownloadLogTombstones;
        private Dictionary<IEntityType, int> _syncUploadLog;
        private Dictionary<IEntityType, int> _syncUploadLogTombstones;        
        private int _threadId;

        public Logger(TimeSpan? logLifetime, int? logMinCount)
        {
            _logLifetime = logLifetime ?? _defaultLogLifetime;
            _logMinCount = logMinCount ?? DefaultLogMinCount;            
            ApplicationContext.Current.InvokeOnMainThread(() => _threadId = Thread.CurrentThread.ManagedThreadId);
        }

        public void ClearLog()
        {
            var db = DbContext.Current.Database;
            string q = string.Format("DELETE FROM {0}", db.LogTable);
            db.ExecuteNonQuery(q);
        }

        public void ScheduledClear()
        {
            if (NeedToClear())
            {
                var db = DbContext.Current.Database;
                string q = string.Format("DELETE FROM {0} WHERE Date < @p1", db.LogTable);
                string p = (DateTime.UtcNow - _logLifetime).ToString("O");
                Console.WriteLine(p);
                int r = db.ExecuteNonQuery(q, p);
                Console.WriteLine(r);
            }
        }

        public IEnumerable<ILog> GetLogs()
        {
            var result = new List<ILog>();

            var db = DbContext.Current.Database;
            if (db != null)
            {
                string query = string.Format("SELECT Date, Event, Content FROM {0} ORDER BY Date DESC LIMIT 1000", db.LogTable);
                using (var reader = db.Select(query))
                {
                    while (reader.Read())
                    {
                        var log = new Log(DateTime.Parse(reader.GetString(0)), reader.GetString(1), reader.GetString(2));
                        result.Add(log);
                    }
                }
            }
            return result;
        }

        public void ScreenOpening(string screenName, string controllerName, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            string parametersString = GetParametersString(parameters);
            Save("screen_opening", "{0}; {1}; {2}", screenName, controllerName, parametersString);
        }

        public void ScreenOpened()
        {
            Save("screen_opened");
        }

        public void SyncStarted()
        {
            _syncDownloadLog = new Dictionary<IEntityType, int>();
            _syncDownloadLogTombstones = new Dictionary<IEntityType, int>();
            _syncUploadLog = new Dictionary<IEntityType, int>();
            _syncUploadLogTombstones = new Dictionary<IEntityType, int>();

            Save("sync_started");
        }

        public void SyncDownload(IEntityType type, bool isTombstone = false)
        {
            AddSyncEvent(type, isTombstone ? _syncDownloadLogTombstones : _syncDownloadLog);
        }

        public void SyncUpload(IEntityType type, bool isTombstone = false)
        {
            AddSyncEvent(type, isTombstone ? _syncUploadLogTombstones : _syncUploadLog);
        }

        public void SyncFinished()
        {
            Save("sync_download", GetSyncContent(_syncDownloadLog));
            Save("sync_download_tombstone", GetSyncContent(_syncDownloadLogTombstones));
            Save("sync_upload", GetSyncContent(_syncUploadLog));
            Save("sync_upload_tombstone", GetSyncContent(_syncUploadLogTombstones));
            Save("sync_finished");

            _syncDownloadLog = null;
            _syncDownloadLogTombstones = null;
            _syncUploadLog = null;
            _syncUploadLogTombstones = null;
        }

        public void ApplicationStarted()
        {
            _enabled = true;
            Save("application_started");
        }

        public void ApplicationClosed()
        {
            Save("application_closed");
            _enabled = false;
        }

        public void ApplicationMinimized()
        {
            Save("application_minimized");
        }

        public void ApplicationMaximized()
        {
            Save("application_maximized");
        }

        public void WorkflowStarted(string name)
        {
            Save("workflow_started", name);
        }

        public void WorkflowPaused()
        {
            Save("workflow_paused");
        }

        public void WorkflowFinished(string reason)
        {
            Save("workflow_finished", reason);
        }

        public void WorkflowForward(string nextStep, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            string parametersString = GetParametersString(parameters);
            Save("workflow_forward", "{0}; {1}", nextStep, parametersString);
        }

        public void WorkflowForwardNotAllowed(string nextStep, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            string parametersString = GetParametersString(parameters);
            Save("workflow_forward_not_allowed", "{0}; {1}", nextStep, parametersString);
        }

        public void WorkflowBack()
        {
            Save("workflow_back");
        }

        public void WorkflowBackTo(string step)
        {
            Save("workflow_back_to", step);
        }

        public void TextInput(string id, string text)
        {
            Save("text_input", "{0}; {1};", id ?? IdIsEmpty, text);
        }

        public void Clicked(string id, string expression, string caption = "")
        {
            Save("clicked", "{0}; {1}; {2}", id ?? IdIsEmpty, expression, caption);
        }

        public void Error(string text, bool isFatal)
        {
            Save(isFatal ? "crash" : "error", text);
        }

        private void Save(string eventType, string content = "", params object[] parameters)
        {
            if (_enabled)
            {
                content = content.Replace('\'', ' ');
                string c = parameters.Length > 0 ? string.Format(content, parameters) : content;

                IDatabase db = DbContext.Current.Database;
                string q = string.Format("INSERT INTO {0}(Date, Event, Content) VALUES(@p1, @p2, @p3);", db.LogTable);

                string now = DateTime.UtcNow.ToString("O");

#if DEBUG
                Console.WriteLine("-> Log: {0} - {1}", eventType, c);
#endif
                if (Thread.CurrentThread.ManagedThreadId != _threadId)
                    ApplicationContext.Current.InvokeOnMainThread(() => db.ExecuteNonQuery(q, now, eventType, c));
                else
                    db.ExecuteNonQuery(q, now, eventType, c);
            }
        }

        private static string GetParametersString(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var p in parameters)
                if (p.Value != null)
                {
                    var loggable = p.Value as ILoggable;
                    string log = loggable != null ? loggable.GetString() : p.Value.ToString();
                    builder.Append(string.Format("{0};", log));
                }

            return builder.ToString();
        }

        private static void AddSyncEvent(IEntityType type, Dictionary<IEntityType, int> collection)
        {
            if (collection.ContainsKey(type))
                collection[type]++;
            else
                collection.Add(type, 1);
        }

        private static string GetSyncContent(Dictionary<IEntityType, int> collection)
        {
            var content = new StringBuilder();
            foreach (var pair in collection)
            {
                string item = string.Format(" {0} {1};", pair.Key.TableName, pair.Value);
                content.Append(item);
            }
            return content.ToString();
        }

        private bool NeedToClear()
        {
            var db = DbContext.Current.Database;
            object result = db.SelectScalar(string.Format("SELECT COUNT(*) FROM {0}", db.LogTable));
            if (result is long)
            {
                var count = (long)result;
                if (count > _logMinCount)
                {
                    result = db.SelectScalar(string.Format("SELECT MIN(Date) FROM {0}", db.LogTable));
                    var dateString = result as string;

                    DateTime date;
                    if (dateString != null && DateTime.TryParse(dateString, out date))
                    {
                        if (DateTime.Now - date > _logLifetime)
                            return true;
                        return false;
                    }

                    Error("SELECT MIN(Date) in Logger.ScheduledClear returns unexpected result: " + result, false);
                    return false;
                }
                return false;
            }

            Error("SELECT COUNT(*) in Logger.ScheduledClear returns unexpected result: " + result, false);
            return false;
        }

        struct Log : ILog
        {
            public DateTime Date { get; private set; }
            public string Event { get; private set; }
            public string Content { get; private set; }

            public Log(DateTime date, string @event, string content)
                : this()
            {
                Date = date;
                Event = @event;
                Content = content;
            }
        }
    }
}
