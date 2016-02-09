using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Synchronization.ClientServices.IsolatedStorage;

using BitMobile.Utilities.Exceptions;
using System.Threading;
using BitMobile.Utilities.Translator;
using System.Net;
using Microsoft.Synchronization.ClientServices;
using BitMobile.DbEngine;

namespace BitMobile.DataAccessLayer
{
    public partial class DAL : IDisposable
    {
        IsolatedStorageOfflineContext context;
        ProgressDelegate onProgress;
        String appName;
        String language;
        String configName;
        String configVersion;

        bool inSync = false;

        ManualResetEventSlim _syncEvent = new ManualResetEventSlim(true);

        event SyncEventHandler syncEvent;
        bool _syncAfterLoad = false;

        public DAL(IsolatedStorageOfflineContext context
            , String appName
            , String language
            , String userName
            , String userPassword
            , String configName
            , String configVersion
            , IDictionary<string, string> deviceInfo
            , ProgressDelegate p
            , Func<Uri, CacheControllerBehavior, AsyncWorkerManager, CacheRequestHandler> chacheRequestFactory)
        {
            this.appName = appName;
            this.language = language;
            this.configName = configName;
            this.configVersion = configVersion;
            this.onProgress = p;

            this.context = context;
            this.context.LoadCompleted += context_LoadCompleted;
            this.context.CacheController.SetCacheRequestFactory(chacheRequestFactory);
            this.context.CacheController.RefreshCompleted += CacheController_RefreshCompleted;
            this.context.CacheController.ControllerBehavior.Credentials = new System.Net.NetworkCredential(userName, userPassword);
            this.context.CacheController.ControllerBehavior.ConfigName = this.configName;
            this.context.CacheController.ControllerBehavior.ConfigVersion = this.configVersion;
            this.context.CacheController.ControllerBehavior.CoreVersion = CoreInformation.CoreVersion;
            this.context.CacheController.ControllerBehavior.ReadProgressCallback = ReadProgressCallback;
            this.context.CacheController.ControllerBehavior.DeviceInfo = deviceInfo;

            //this.context.CacheController.ControllerBehavior.AddScopeParameters("Outlet","{A507956F-D135-4E8A-BD7F-14B54AC1E95C}");
        }

        public void UpdateCredentials(String userName, String password)
        {
            context.CacheController.ControllerBehavior.Credentials = new System.Net.NetworkCredential(userName, password);
        }

        public Guid UserId
        {
            get
            {
                return new Guid(Database.Current.UserId);
            }
        }

        public string UserEmail
        {
            get
            {
                return Database.Current.UserEmail;
            }
        }

        public String ConfigName
        {
            get
            {
                return configName;
            }
        }

        public String ConfigVersion
        {
            get
            {
                return configVersion;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            if (context != null)
                context.Dispose();
        }
        #endregion

        private void ReadProgressCallback(int total, int processed)
        {
            if (onProgress != null)
                onProgress(total, processed);
        }

        private void CacheController_RefreshCompleted(object sender, Microsoft.Synchronization.ClientServices.RefreshCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is System.Net.WebException)
                {
                    System.Net.WebException we = (System.Net.WebException)e.Error;
                    if (we.Response != null)
                    {
                        System.IO.Stream stream = we.Response.GetResponseStream();
                        System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                        String errorMessage = reader.ReadToEnd();

                        try
                        {
                            if (errorMessage.StartsWith("<ServiceError") || errorMessage.StartsWith("<?xml"))
                            {
                                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                doc.LoadXml(errorMessage);
                                errorMessage = doc.DocumentElement.InnerText;
                            }
                        }
                        catch
                        {
                        }

                        CustomException exc = HandleStatusCode(((System.Net.HttpWebResponse)we.Response).StatusCode, we.Message, errorMessage, e.Error);
                        RefreshComplete(exc);
                    }
                    else
                        RefreshComplete(new ConnectionException("WebException has been thrown during the refresh operation", e.Error));
                }
                else if (e.Error is CacheControllerWebException)
                {
                    CacheControllerWebException we = (CacheControllerWebException)e.Error;
                    CustomException exc = HandleStatusCode(we.StatusCode, we.Message);
                    RefreshComplete(exc);
                }
                else
                    RefreshComplete(new NonFatalException(D.CONNECTION_EXCEPTION, e.Error.Message, e.Error));
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
                    result = new InternalServerException(message, D.SERVER_CONFIG_WAS_CHANGED);
                    result.Report = null;
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

        private void context_LoadCompleted(object sender, Microsoft.Synchronization.ClientServices.IsolatedStorage.LoadCompletedEventArgs e)
        {
            if (_syncAfterLoad)
            {
                RefreshAsync(syncEvent);
                _syncAfterLoad = false;
            }
            else
                if (syncEvent != null)
                    syncEvent(this, new SyncEventArgs());
        }


        public void LoadSolution(bool clearCache, bool syncAfterLoad, SyncEventHandler handler)
        {
            _syncAfterLoad = syncAfterLoad;

            if (!clearCache && !DbEngine.Database.Current.IsSynced())
                clearCache = true;

            if (clearCache)
                DbEngine.Database.CreateDatabase();

            if (!clearCache)
            {
                DbEngine.Database.Current.OnStart();
                LoadAsync(handler);
            }
            else
            {
                RefreshAsync(handler);
            }
        }

        void LoadAsync(SyncEventHandler handler)
        {
            this.syncEvent = handler;
            context.LoadAsync();

        }

        void ClearCache()
        {
            this.context.CacheController.ControllerBehavior.ClearUserSession();
            context.ClearCache();
        }

        public void RefreshAsync(SyncEventHandler handler)
        {
            if (!inSync)
            {
                if (DbEngine.Database.Current.InTransaction())
                    throw new Exception("You shoud rollback or commit transaction before sync");

                inSync = true;
                _syncEvent.Reset();
                this.syncEvent = handler;
                context.CacheController.RefreshAsync();
            }
        }

        void RefreshComplete(Exception exception = null)
        {
            inSync = false;
            _syncEvent.Set();
            if (syncEvent != null)
                syncEvent(this, new SyncEventArgs(exception));
        }

        public void Wait()
        {
            _syncEvent.Wait();
        }

        public void SaveChanges()
        {
            DbEngine.Database.Current.CommitTransaction();
            //context.SaveChanges();
        }

        public void CancelChanges()
        {
            DbEngine.Database.Current.RollbackTransaction();
            //context.CancelChanges();
        }

        public Microsoft.Synchronization.ClientServices.IsolatedStorage.IsolatedStorageOfflineContext DAO
        {
            get
            {
                return context;
            }
        }
    }

    public class SyncEventArgs
    {
        public SyncEventArgs(Exception exception = null)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }

        public bool OK
        {
            get
            {
                return this.Exception == null;
            }
        }
    }

    public delegate void SyncEventHandler(object sender, SyncEventArgs e);
    public delegate void ProgressDelegate(int total, int processed);
}