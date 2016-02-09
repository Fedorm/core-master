using System;
using System.Collections.Generic;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using System.Threading;
using System.Net;
using BitMobile.Common;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.SyncLibrary;

namespace BitMobile.DataAccessLayer
{
    public partial class Dal : IDal, IDisposable
    {
        private readonly IOfflineContext _context;
        private readonly Action<int, int> _onProgress;
        private readonly String _appName;
        private readonly String _language;
        private readonly String _configName;
        private readonly String _configVersion;
        private readonly ManualResetEventSlim _syncEvent = new ManualResetEventSlim(true);
        private bool _inSync;
        private bool _syncAfterLoad;
        private event SyncEventHandler SyncEvent;
        private bool _started;

        public Dal(IOfflineContext context
            , String appName
            , String language
            , String userName
            , String userPassword
            , String configName
            , String configVersion
            , IDictionary<string, string> deviceInfo
            , Action<int, int> p)
        {
            ServicePointManager.Expect100Continue = false;

            _appName = appName;
            _language = language;
            _configName = configName;
            _configVersion = configVersion;
            _onProgress = p;

            _context = context;
            _context.LoadCompleted += context_LoadCompleted;
            _context.CacheController.RefreshCompleted += CacheController_RefreshCompleted;
            _context.CacheController.ControllerBehavior.Credentials = new NetworkCredential(userName, userPassword);
            _context.CacheController.ControllerBehavior.ConfigName = _configName;
            _context.CacheController.ControllerBehavior.ConfigVersion = _configVersion;
            _context.CacheController.ControllerBehavior.CoreVersion = CoreInformation.CoreVersion;
            _context.CacheController.ControllerBehavior.ReadProgressCallback = ReadProgressCallback;
            _context.CacheController.ControllerBehavior.DeviceInfo = deviceInfo;

            DeviceId = deviceInfo["deviceId"];
        }

        public void UpdateCredentials(String userName, String password)
        {
            _context.CacheController.ControllerBehavior.Credentials = new NetworkCredential(userName, password);
        }

        public Guid UserId
        {
            get
            {
                return new Guid(DbContext.Current.Database.UserId);
            }
        }

        public string DeviceId { get; private set; }

        public string UserEmail
        {
            get
            {
                return DbContext.Current.Database.UserEmail;
            }
        }

        public String ConfigName
        {
            get
            {
                return _configName;
            }
        }

        public String ConfigVersion
        {
            get
            {
                return _configVersion;
            }
        }

        public string ResourceVersion
        {
            get { return DbContext.Current.Database.ResourceVersion; }
        }

        #region IDisposable

        public void Dispose()
        {
            if (_context != null)
                _context.Dispose();
        }
        #endregion
        
        private void ReadProgressCallback(int total, int processed)
        {
            if (_onProgress != null)
                _onProgress(total, processed);
        }

        private void CacheController_RefreshCompleted(Exception e)
        {
            if (e != null)
            {
                var we = e as WebException;
                if (we != null)
                {
                    if (we.Response != null)
                    {
                        System.IO.Stream stream = we.Response.GetResponseStream();
                        var reader = new System.IO.StreamReader(stream);
                        String errorMessage = reader.ReadToEnd();

                        try
                        {
                            if (errorMessage.StartsWith("<ServiceError") || errorMessage.StartsWith("<?xml"))
                            {
                                var doc = new System.Xml.XmlDocument();
                                doc.LoadXml(errorMessage);
                                errorMessage = doc.DocumentElement.InnerText;
                            }
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }

                        CustomException exc = HandleStatusCode(((HttpWebResponse)we.Response).StatusCode, we.Message, errorMessage, e);
                        RefreshComplete(exc);
                    }
                    else
                        RefreshComplete(new ConnectionException("WebException has been thrown during the refresh operation", e));
                }
                else
                {
                    var ccwe = e as CacheControllerWebException;
                    if (ccwe != null)
                    {
                        CustomException exc = HandleStatusCode(ccwe.StatusCode, ccwe.Message, ccwe.Message, e);
                        RefreshComplete(exc);
                    }
                    else
                        RefreshComplete(new NonFatalException(D.CONNECTION_EXCEPTION, e.Message, e));
                }
            }
            else
            {
                RefreshComplete();
            }
        }

        CustomException HandleStatusCode(HttpStatusCode code, string message, string serverMessage = "", Exception innerException = null)
        {
            CustomException result;
            switch (code)
            {
                case HttpStatusCode.Unauthorized:
                    result = new AutorizationException(message);
                    break;
                case HttpStatusCode.PaymentRequired:
                    result = new LicenseException();
                    break;
                case HttpStatusCode.PreconditionFailed:
                    result = new InvalidVersionException(message);
                    break;
                case HttpStatusCode.Conflict:
                    result = new InternalServerException(message, D.SERVER_CONFIG_WAS_CHANGED) { Report = null };
                    break;
                case HttpStatusCode.NotAcceptable:
                    result = new InvalidVersionException(message);
                    break;
                default:
                    result = new InternalServerException(serverMessage, innerException);
                    break;
            }
            return result;
        }

        private void context_LoadCompleted(Exception e)
        {
            // TODO: разобраться, что за хрень: почему параметр е никуда не передается?

            LogManager.Logger.ApplicationStarted();
            _started = true;

            if (_syncAfterLoad)
            {
                RefreshAsync(SyncEvent);
                _syncAfterLoad = false;
            }
            else
                if (SyncEvent != null)
                SyncEvent(this, new SyncEventArgs());
        }


        public void LoadSolution(bool clearCache, bool syncAfterLoad, SyncEventHandler handler)
        {
            _syncAfterLoad = syncAfterLoad;

            if (!clearCache && !DbContext.Current.Database.IsSynced())
                clearCache = true;

            if (clearCache)
                DbContext.Current.CreateDatabase();
            else
                LogManager.Logger.ScheduledClear();

            if (!clearCache)
            {
                DbContext.Current.Database.OnStart();
                LoadAsync(handler);
            }
            else
            {
                RefreshAsync(handler);
            }
        }

        void LoadAsync(SyncEventHandler handler)
        {
            SyncEvent = handler;
            _context.LoadAsync();
        }

        void ClearCache()
        {
            _context.CacheController.ControllerBehavior.ClearUserSession();
            _context.ClearCache();
        }

        public void RefreshAsync(SyncEventHandler handler)
        {
            if (!_inSync)
            {
                if (DbContext.Current.Database.InTransaction())
                    throw new Exception("You shoud rollback or commit transaction before sync");

                LogManager.Logger.SyncStarted();

                _inSync = true;
                _syncEvent.Reset();
                SyncEvent = handler;
                _context.CacheController.RefreshAsync();
            }
        }

        void RefreshComplete(Exception exception = null)
        {
            LogManager.Logger.SyncFinished();

            if (!_started)
            {
                LogManager.Logger.ApplicationStarted();
                _started = true;
            }

            _inSync = false;
            _syncEvent.Set();
            if (SyncEvent != null)
                SyncEvent(this, new SyncEventArgs(exception));
        }

        public void Wait()
        {
            _syncEvent.Wait();
        }

        public void SaveChanges()
        {
            DbContext.Current.Database.CommitTransaction();
        }

        public void CancelChanges()
        {
            DbContext.Current.Database.RollbackTransaction();
        }

        public object Dao
        {
            get
            {
                return _context;
            }
        }
    }

    public class SyncEventArgs : ISyncEventArgs
    {
        public SyncEventArgs(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }

        public bool Ok
        {
            get
            {
                return Exception == null;
            }
        }
    }
}