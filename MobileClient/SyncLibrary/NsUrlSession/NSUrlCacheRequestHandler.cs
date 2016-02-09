using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Runtime.Serialization.Json;
using BitMobile.Application.Entites;
using BitMobile.Application.IO;
using BitMobile.Common.IO;
using MonoTouch.Foundation;
using System.Net;
using MonoTouch.UIKit;
using Microsoft.Synchronization.Services.Formatters;
using BitMobile.Application.Log;
using BitMobile.Common.ValueStack;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using BitMobile.Application;
using BitMobile.Common.SyncLibrary;

namespace Microsoft.Synchronization.ClientServices
{
    sealed class NSUrlCacheRequestHandler : CacheRequestHandler, IDisposable
    {
        const int Timeout = 10 * 60;

        readonly object _lockObject;
        readonly AsyncWorkerManager _workerManager;
        readonly EntityType[] _knownTypes;
        readonly ICredentials _credentials;
        readonly Dictionary<string, string> _scopeParameters;
        readonly CacheControllerBehavior _behaviors;
        SyncReader _syncReader;
        SyncWriter _syncWriter;
        AsyncArgsWrapper _wrapper;
        NSUrlSessionDownloadTask _currentTask;
        NSUrlSession _downloadSession;
        NSUrlSession _uploadSession;
        string _filePath;
        bool _disposed;

        public NSUrlCacheRequestHandler(Uri serviceUri, CacheControllerBehavior behaviors, AsyncWorkerManager manager)
            : base(serviceUri, behaviors.SerializationFormat, behaviors.ScopeName)
        {
            _workerManager = manager;
            _scopeParameters = new Dictionary<string, string>(behaviors.ScopeParametersInternal);
            _knownTypes = new EntityType[behaviors.KnownTypes.Count];
            behaviors.KnownTypes.CopyTo(_knownTypes, 0);
            _lockObject = new object();

            _credentials = behaviors.Credentials;
            _behaviors = behaviors;
        }

        #region IDisposable implementation

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_lockObject)
                {
                    if (_downloadSession != null)
                    {
                        _downloadSession.Dispose();
                        _downloadSession = null;
                    }

                    if (_uploadSession != null)
                    {
                        _uploadSession.Dispose();
                        _uploadSession = null;
                    }
                }
            }

            _disposed = true;
        }

        ~NSUrlCacheRequestHandler()
        {
            Dispose(false);
        }

        #endregion

        public override void ProcessCacheRequestAsync(CacheRequest request, object state)
        {
            _workerManager.AddWorkRequest(new AsyncWorkRequest(ProcessCacheRequestWorker, CacheRequestCompleted, request, state));
        }

        void ProcessCacheRequestWorker(AsyncWorkRequest worker, object[] inputParams)
        {
            var request = inputParams[0] as CacheRequest;
            object state = inputParams[1];

            _wrapper = new AsyncArgsWrapper
            {
                UserPassedState = state,
                WorkerRequest = worker,
                CacheRequest = request
            };

            ProcessRequest();
        }

        void ProcessRequest()
        {
            try
            {
                var requestUri = new StringBuilder();
                requestUri.AppendFormat("{0}{1}{2}/{3}",
                    BaseUri,
                    (BaseUri.ToString().EndsWith("/")) ? string.Empty : "/",
                    Uri.EscapeUriString(ScopeName),
                    _wrapper.CacheRequest.RequestType.ToString());

                string prefix = "?";

                // Add the scope params if any
                foreach (KeyValuePair<string, string> kvp in _scopeParameters)
                {
                    requestUri.AppendFormat("{0}{1}={2}", prefix, Uri.EscapeUriString(kvp.Key), Uri.EscapeUriString(kvp.Value));
                    if (prefix.Equals("?"))
                    {
                        prefix = "&";
                    }
                }

                // Create the WebRequest
                var webRequest = new NSMutableUrlRequest(new NSUrl(requestUri.ToString()));
                if (_credentials != null)
                {
                    NetworkCredential credential = _credentials.GetCredential(BaseUri, "Basic");
                    string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credential.UserName + ":" + credential.Password));
                    webRequest["Authorization"] = "Basic " + svcCredentials;

                    webRequest["configname"] = _behaviors.ConfigName;
                    webRequest["configversion"] = _behaviors.ConfigVersion;

                    webRequest["coreversion"] = _behaviors.CoreVersion.ToString();
                }
                else
                    throw new Exception("Credentials is null");

                foreach (var item in _behaviors.DeviceInfo)
                    webRequest[item.Key] = item.Value;

                webRequest.HttpMethod = "POST";
                webRequest["Accept"] = ApplicationContext.Current.Settings.BitMobileFormatterDisabled ? "application/atom+xml" : "application/bitmobile";
                webRequest["Content-Type"] = ApplicationContext.Current.Settings.BitMobileFormatterDisabled ? "application/atom+xml" : "application/bitmobile";
                webRequest["Accept-Encoding"] = "gzip, deflate";
                webRequest.TimeoutInterval = Timeout;

                webRequest.Body = CreateRequestBody();

                _wrapper.WebRequest = webRequest;

                if (_wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                {
                    lock (_lockObject)
                    {
                        _currentTask = CreateUploadSession().CreateDownloadTask(webRequest);
                        _currentTask.Resume();
                    }
                }
                else
                {
                    lock (_lockObject)
                    {
                        _currentTask = CreateDownloadSession().CreateDownloadTask(webRequest);
                        _currentTask.Resume();
                    }
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                    throw;

                _wrapper.Error = e;

                _workerManager.CompleteWorkRequest(_wrapper.WorkerRequest, _wrapper);
            }
        }

        NSUrlSession CreateDownloadSession()
        {
            if (_downloadSession == null)
            {
                string urlSessioinId = Guid.NewGuid().ToString();
                NSUrlSessionConfiguration sessionConfiguration;
                sessionConfiguration = new Version(UIDevice.CurrentDevice.SystemVersion).Major > 7
                    ? NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(urlSessioinId)
                    : NSUrlSessionConfiguration.BackgroundSessionConfiguration(urlSessioinId);
                sessionConfiguration.TimeoutIntervalForRequest = Timeout;
                sessionConfiguration.TimeoutIntervalForResource = Timeout;

                var downloadDelegate = new NSUrlDownloadDelegate(OnDownloadCompleted, OnProgress);

                _downloadSession = NSUrlSession.FromConfiguration(sessionConfiguration, downloadDelegate, new NSOperationQueue());

            }

            return _downloadSession;
        }

        NSUrlSession CreateUploadSession()
        {
            if (_uploadSession == null)
            {
                string urlSessioinId = Guid.NewGuid().ToString();
                NSUrlSessionConfiguration sessionConfiguration;
                sessionConfiguration = new Version(UIDevice.CurrentDevice.SystemVersion).Major > 7
                    ? NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(urlSessioinId)
                    : NSUrlSessionConfiguration.BackgroundSessionConfiguration(urlSessioinId);
                sessionConfiguration.TimeoutIntervalForRequest = Timeout;
                sessionConfiguration.TimeoutIntervalForResource = Timeout;

                var uploadDelegate = new NSUrlUploadDelegate(OnUploadCompleted, _behaviors.ReadProgressCallback);

                _uploadSession = NSUrlSession.FromConfiguration(sessionConfiguration, uploadDelegate, new NSOperationQueue());
            }

            return _uploadSession;
        }

        void OnProgress(int totalBytes, int processed)
        {
            NSDictionary headers = ((NSHttpUrlResponse)_currentTask.Response).AllHeaderFields;
            int total;
            NSObject lengthHeader = headers["unzippedcontentlength"];
            if (lengthHeader != null && int.TryParse(lengthHeader.ToString(), out total))
            {
                totalBytes = total;
            }
            _behaviors.ReadProgressCallback(totalBytes, processed);
        }

        NSData CreateRequestBody()
        {
            if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                _syncWriter = new ODataAtomWriter(BaseUri);
            else
                _syncWriter = new BMWriter(BaseUri);

            _syncWriter.StartFeed(_wrapper.CacheRequest.IsLastBatch, _wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

            var requestStream = new MemoryStream();

            if (_wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
            {
                foreach (IOfflineEntity entity in _wrapper.CacheRequest.Changes)
                {
                    var ientity = (IEntity)entity;
                    // Skip tombstones that dont have a ID element.
                    if (entity.ServiceMetadata.IsTombstone && string.IsNullOrEmpty(entity.ServiceMetadata.Id))
                    {
                        if (ientity != null)
                            LogManager.Logger.SyncUpload(ientity.EntityType, true);
                        continue;
                    }

                    string tempId = null;

                    // Check to see if this is an insert. i.e ServiceMetadata.Id is null or empty
                    if (string.IsNullOrEmpty(entity.ServiceMetadata.Id))
                    {
                        if (_wrapper.TempIdToEntityMapping == null)
                        {
                            _wrapper.TempIdToEntityMapping = new Dictionary<string, IOfflineEntity>();
                        }

                        tempId = Guid.NewGuid().ToString();

                        _wrapper.TempIdToEntityMapping.Add(tempId, entity);
                    }

                    _syncWriter.AddItem(entity, tempId);

                    if (ientity != null)
                        LogManager.Logger.SyncUpload(ientity.EntityType);
                }
            }

            _syncWriter.WriteFeed(SerializationFormat == SerializationFormat.ODataAtom
                ? XmlWriter.Create(requestStream)
                : JsonReaderWriterFactory.CreateJsonWriter(requestStream));

            string result;

            requestStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(requestStream))
            {
                result = reader.ReadToEnd();
            }

            requestStream.Flush();
            requestStream.Close();

            var dataString = new NSString(result);

            return dataString.DataUsingEncoding(NSStringEncoding.UTF8);
        }

        void CacheRequestCompleted(object state)
        {
            // Fire the ProcessCacheRequestCompleted handler
            var wrapper = (AsyncArgsWrapper)state;
            if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
            {
                OnProcessCacheRequestCompleted(
                    new ProcessCacheRequestCompletedEventArgs(
                        wrapper.CacheRequest.RequestId,
                        wrapper.UploadResponse,
                        wrapper.Error,
                        wrapper.UserPassedState)
                );
            }
            else
            {
                OnProcessCacheRequestCompleted(
                    new ProcessCacheRequestCompletedEventArgs(
                        wrapper.CacheRequest.RequestId,
                        wrapper.DownloadResponse,
                        wrapper.Error,
                        wrapper.UserPassedState)
                );
            }
        }

        void OnUploadCompleted(object sender, NSUrlEventArgs e)
        {
            _wrapper.UploadResponse = new ChangeSetResponse();

            FileStream fileStream = null;

            string filePath = null;

            try
            {
                if (e.Error == null)
                {
                    string responseDescription = "response is  null";
                    var response = (NSHttpUrlResponse)_currentTask.Response;
                    if (response != null)
                        responseDescription = response.Description;

                    filePath = e.FilePath.Replace("file://", "").Replace("%20", " ");
                    IIOContext io = IOContext.Current;
                    if (io.Exists(filePath))
                    {
                        fileStream = io.FileStream(filePath, FileMode.Open);
                        fileStream.Seek(0, SeekOrigin.Begin);

                        // CreateInstance the SyncReader
                        if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                            _syncReader = new ODataAtomReader(fileStream, _knownTypes);
                        else
                            _syncReader = new BMReader(fileStream, _knownTypes);

                        // Read the response
                        while (_syncReader.Next())
                        {
                            switch (_syncReader.ItemType)
                            {
                                case ReaderItemType.Entry:
                                    IOfflineEntity entity = _syncReader.GetItem();
                                    IOfflineEntity ackedEntity = entity;
                                    string tempId = null;

                                    if (_syncReader.HasTempId() && _syncReader.HasConflictTempId())
                                    {
                                        throw new CacheControllerException(string.Format("Service returned a TempId '{0}' in both live and conflicting entities.",
                                            _syncReader.GetTempId()));
                                    }

                                    if (_syncReader.HasTempId())
                                    {
                                        tempId = _syncReader.GetTempId();
                                        CheckEntityServiceMetadataAndTempIds(entity, tempId);
                                    }

                                    if (_syncReader.HasConflict())
                                    {
                                        Conflict conflict = _syncReader.GetConflict();
                                        IOfflineEntity conflictEntity = (conflict is SyncConflict) ?
                                                                        ((SyncConflict)conflict).LosingEntity : ((SyncError)conflict).ErrorEntity;

                                        if (_syncReader.HasConflictTempId())
                                        {
                                            tempId = _syncReader.GetConflictTempId();
                                            CheckEntityServiceMetadataAndTempIds(conflictEntity, tempId);
                                        }

                                        _wrapper.UploadResponse.AddConflict(conflict);

                                        if (_syncReader.HasConflictTempId() && entity.ServiceMetadata.IsTombstone)
                                        {
                                            conflictEntity.ServiceMetadata.IsTombstone = true;
                                            ackedEntity = conflictEntity;
                                        }
                                    }

                                    if (!String.IsNullOrEmpty(tempId))
                                    {
                                        _wrapper.UploadResponse.AddUpdatedItem(ackedEntity);
                                    }

                                    break;

                                case ReaderItemType.SyncBlob:
                                    _wrapper.UploadResponse.ServerBlob = _syncReader.GetServerBlob();
                                    break;
                            }
                        }

                    }
                    else
                        _wrapper.Error = new FileNotFoundException(String.Format("Downloaded data file not found! {0}, Description: {1}", e.FilePath, responseDescription));
                }
                else
                {
                    var response = _currentTask.Response as NSHttpUrlResponse;
                    HandleError(e.Error, response);
                }

                _workerManager.CompleteWorkRequest(_wrapper.WorkerRequest, _wrapper);
            }
            catch (Exception ex)
            {
                if (ExceptionUtility.IsFatal(ex))
                {
                    throw;
                }

                _wrapper.Error = ex;

                _workerManager.CompleteWorkRequest(_wrapper.WorkerRequest, _wrapper);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                if (filePath != null)
                    IOContext.Current.Delete(filePath);
            }
        }

        void OnDownloadCompleted(object sender, NSUrlEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    _filePath = e.FilePath.Replace("file://", "").Replace("%20", " ");

                    NSHttpUrlResponse response = (NSHttpUrlResponse)_currentTask.Response;
                    if (response != null)
                    {

                        HttpStatusCode code = (HttpStatusCode)response.StatusCode;
                        if (code == HttpStatusCode.OK)
                        {

                            NSDictionary headers = response.AllHeaderFields;
                            if (string.IsNullOrWhiteSpace(_behaviors.UserId))
                                _behaviors.UserId = headers["userid"].ToString();
                            if (string.IsNullOrWhiteSpace(_behaviors.UserEmail))
                                _behaviors.UserEmail = headers["email"].ToString();
                            _behaviors.ResourceVersion = headers["resourceversion"].ToString();

                            _behaviors.SaveUserSession();

                            IIOContext io = IOContext.Current;
                            if (io.Exists(_filePath))
                            {
                                FileStream fileStream = io.FileStream(_filePath, FileMode.Open);

                                fileStream.Seek(0, SeekOrigin.Begin);
                                int contentLength;
                                if (!int.TryParse(headers["unzippedcontentlength"].ToString(), out contentLength))
                                    contentLength = -1;
                                Stream responseStream = new ProgressStream(fileStream, contentLength, _behaviors.ReadProgressCallback);

                                // CreateInstance the SyncReader
                                if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                                    _syncReader = new ODataAtomReader(responseStream, _knownTypes);
                                else
                                    _syncReader = new BMReader(responseStream, _knownTypes);

                                _wrapper.DownloadResponse = new ChangeSet();
                                _wrapper.DownloadResponse.Data = GetDownloadedValues(_wrapper);

                            }
                            else
                            {
                                _wrapper.Error = new FileNotFoundException(String.Format("Downloaded data file not found! {0}, Description: {1}", e.FilePath, response.Description));
                            }
                        }
                        else
                            _wrapper.Error = new CacheControllerWebException(string.Format("Remote service returned error status. Status: {0}, Description: {1}", code, response.Description), code);
                    }
                    else
                        _wrapper.Error = new CacheControllerException("Response is null");
                }
                else
                {
                    var response = _currentTask.Response as NSHttpUrlResponse;
                    HandleError(e.Error, response);
                }

                // If we get here then it means we completed the request. Return to the original caller
                _workerManager.CompleteWorkRequest(_wrapper.WorkerRequest, _wrapper);

            }
            catch (Exception ex)
            {
                if (ExceptionUtility.IsFatal(ex))
                {
                    throw;
                }

                _wrapper.Error = ex;

                _workerManager.CompleteWorkRequest(_wrapper.WorkerRequest, _wrapper);
            }
        }

        private IEnumerable<IsolatedStorageOfflineEntity> GetDownloadedValues(AsyncArgsWrapper wrapper)
        {
            // Read the response
            while (_syncReader.Next())
            {
                switch (_syncReader.ItemType)
                {
                    case ReaderItemType.Entry:
                        IsolatedStorageOfflineEntity offlineEntity = _syncReader.GetItem();
                        var entity = offlineEntity as IEntity;
                        if (entity != null)
                            LogManager.Logger.SyncDownload(entity.EntityType, offlineEntity.ServiceMetadata.IsTombstone);
                        yield return offlineEntity;
                        break;
                    case ReaderItemType.SyncBlob:
                        _wrapper.DownloadResponse.ServerBlob = _syncReader.GetServerBlob();
                        break;
                    case ReaderItemType.HasMoreChanges:
                        _wrapper.DownloadResponse.IsLastBatch = !_syncReader.GetHasMoreChangesValue();
                        break;
                }
            }

            _syncReader.Dispose();

            if (_filePath != null)
                IOContext.Current.Delete(_filePath);
        }


        void HandleError(string error, NSHttpUrlResponse response)
        {
            if (response != null)
            {
                string msg = string.Format("Remote service returned error {0}. Status: {1}, Description: {2}", error, response.StatusCode, response.Description);
                _wrapper.Error = new CacheControllerWebException(msg, (HttpStatusCode)response.StatusCode);
            }
            else
                _wrapper.Error = new CacheControllerException(error);
        }

        void CheckEntityServiceMetadataAndTempIds(IOfflineEntity entity, string tempId)
        {
            // Check service ID 
            if (string.IsNullOrEmpty(entity.ServiceMetadata.Id))
            {
                throw new CacheControllerException(string.Format("Service did not return a permanent Id for tempId '{0}'", tempId));
            }

            // If an entity has a temp id then it should not be a tombstone                
            if (entity.ServiceMetadata.IsTombstone)
            {
                throw new CacheControllerException(string.Format("Service returned a tempId '{0}' in tombstoned entity.", tempId));
            }

            // Check that the tempId was sent by client
            if (!_wrapper.TempIdToEntityMapping.ContainsKey(tempId))
            {
                throw new CacheControllerException("Service returned a response for a tempId which was not uploaded by the client. TempId: " + tempId);
            }

            // Once received, remove the tempId from the mapping list.
            _wrapper.TempIdToEntityMapping.Remove(tempId);
        }

        internal class AsyncArgsWrapper
        {
            public CacheRequest CacheRequest;
            public AsyncWorkRequest WorkerRequest;
            public object UserPassedState;
            public ChangeSetResponse UploadResponse;
            public ChangeSet DownloadResponse;
            public Exception Error;
            public NSUrlRequest WebRequest;
            public Dictionary<string, IOfflineEntity> TempIdToEntityMapping;
        }
    }
}

