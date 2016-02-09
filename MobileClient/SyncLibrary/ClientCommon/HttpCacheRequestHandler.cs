// Copyright 2010 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

//using System.Net.Browser;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.XPath;
using BitMobile.Application;
using BitMobile.Application.Entites;
using BitMobile.Application.Log;
using BitMobile.Common.Entites;
using BitMobile.Common.ValueStack;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using Microsoft.Synchronization.Services.Formatters;
using System.Threading;
using BitMobile.Common.Utils;

namespace Microsoft.Synchronization.ClientServices
{
    /// <summary>
    /// A Http transport implementation for processing a CachedRequest.
    /// </summary>
    public class HttpCacheRequestHandler : CacheRequestHandler
    {
        const int TIMEOUT = 10 * 60 * 1000;

        ICredentials _credentials;
        SyncReader _syncReader;
        SyncWriter _syncWriter;
        Action<HttpWebRequest, Action<HttpWebRequest>> _beforeRequestHandler;
        Action<HttpWebResponse> _afterResponseHandler;
        AsyncWorkerManager _workerManager;
        EntityType[] _knownTypes;
        Dictionary<string, string> _scopeParameters;
        Dictionary<int, AsyncArgsWrapper> _requestToArgsMapper;
        CacheControllerBehavior behaviors;

        object _timeoutSync = new object();

        public HttpCacheRequestHandler(Uri serviceUri, CacheControllerBehavior behaviors, AsyncWorkerManager manager)
            : base(serviceUri, behaviors.SerializationFormat, behaviors.ScopeName)
        {
            this._credentials = behaviors.Credentials;
            this._beforeRequestHandler = behaviors.BeforeSendingRequest;
            this._afterResponseHandler = behaviors.AfterReceivingResponse;
            this._workerManager = manager;
            this._knownTypes = new EntityType[behaviors.KnownTypes.Count];
            behaviors.KnownTypes.CopyTo(_knownTypes, 0);
            this._scopeParameters = new Dictionary<string, string>(behaviors.ScopeParametersInternal);
            this._requestToArgsMapper = new Dictionary<int, AsyncArgsWrapper>();
            this.behaviors = behaviors;
        }

        /// <summary>
        /// Called by the CacheController when it wants this CacheRequest to be processed asynchronously.
        /// </summary>
        /// <param name="request">CacheRequest to be processed</param>
        /// <param name="state">User state object</param>
        public override void ProcessCacheRequestAsync(CacheRequest request, object state)
        {
            this._workerManager.AddWorkRequest(new AsyncWorkRequest(ProcessCacheRequestWorker, CacheRequestCompleted, request, state));
        }

        /// <summary>
        /// Actual worker performing the work
        /// </summary>
        /// <param name="worker">AsyncWorkRequest object</param>
        /// <param name="inputParams">input parameters</param>
        void ProcessCacheRequestWorker(AsyncWorkRequest worker, object[] inputParams)
        {
            Debug.Assert(inputParams.Length == 2);

            CacheRequest request = inputParams[0] as CacheRequest;
            object state = inputParams[1];

            AsyncArgsWrapper wrapper = new AsyncArgsWrapper()
            {
                UserPassedState = state,
                WorkerRequest = worker,
                CacheRequest = request
            };

            ProcessRequest(wrapper);
        }

        /// <summary>
        /// Callback invoked when the cache request has been processed.
        /// </summary>
        /// <param name="state">AsyncArgsWrapper object</param>
        void CacheRequestCompleted(object state)
        {
            // Fire the ProcessCacheRequestCompleted handler
            AsyncArgsWrapper wrapper = state as AsyncArgsWrapper;
            if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
            {
                base.OnProcessCacheRequestCompleted(
                    new ProcessCacheRequestCompletedEventArgs(
                        wrapper.CacheRequest.RequestId,
                        wrapper.UploadResponse,
                        wrapper.Error,
                        wrapper.UserPassedState)
                );
            }
            else
            {
                base.OnProcessCacheRequestCompleted(
                    new ProcessCacheRequestCompletedEventArgs(
                        wrapper.CacheRequest.RequestId,
                        wrapper.DownloadResponse,
                        wrapper.Error,
                        wrapper.UserPassedState)
                );
            }
        }

        /// <summary>
        /// Method that does the actual processing. 
        /// 1. It first creates an HttpWebRequest
        /// 2. Fills in the required method type and parameters.
        /// 3. Attaches the user specified ICredentials.
        /// 4. Serializes the input params (Server blob for downloads and input feed for uploads)
        /// 5. If user has specified an BeforeSendingRequest callback then invokes it
        /// 6. Else proceeds to issue the request
        /// </summary>
        /// <param name="wrapper">AsyncArgsWrapper object</param>
        void ProcessRequest(AsyncArgsWrapper wrapper)
        {
            try
            {
                StringBuilder requestUri = new StringBuilder();
                requestUri.AppendFormat("{0}{1}{2}/{3}",
                                         base.BaseUri,
                                         (base.BaseUri.ToString().EndsWith("/")) ? string.Empty : "/",
                                         Uri.EscapeUriString(base.ScopeName),
                                         wrapper.CacheRequest.RequestType.ToString());

                string prefix = "?";
                // Add the scope params if any
                foreach (KeyValuePair<string, string> kvp in this._scopeParameters)
                {
                    requestUri.AppendFormat("{0}{1}={2}", prefix, Uri.EscapeUriString(kvp.Key), Uri.EscapeUriString(kvp.Value));
                    if (prefix.Equals("?"))
                    {
                        prefix = "&";
                    }
                }

                //requestUri.AppendFormat("{0}{1}={2}", prefix, Uri.EscapeUriString("user"), Uri.EscapeUriString("test"));
                //requestUri.AppendFormat("{0}{1}={2}", "&", Uri.EscapeUriString("pwd"), Uri.EscapeUriString("test"));

                // CreateInstance the WebRequest
                HttpWebRequest webRequest = null;

                if (this._credentials != null)
                {
                    // CreateInstance the Client Http request
                    //webRequest = (HttpWebRequest)WebRequestCreator.ClientHttp.CreateInstance(new Uri(requestUri.ToString()));
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(requestUri.ToString().ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled)));

                    NetworkCredential credential = this._credentials.GetCredential(BaseUri, "Basic");

                    // Add credentials
                    webRequest.Credentials = this._credentials;

                    string svcCredentials =
                        Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(credential.UserName + ":" + credential.Password));
                    webRequest.Headers.Add("Authorization", "Basic " + svcCredentials);

                    webRequest.Headers.Add("configname:" + behaviors.ConfigName);
                    webRequest.Headers.Add("configversion:" + behaviors.ConfigVersion);

                    webRequest.Headers.Add("coreversion", behaviors.CoreVersion.ToString());
                }
                else
                {
                    // Use WebRequest.CreateInstance the request. This uses any user defined prefix preferences for certain paths
                    webRequest = (HttpWebRequest)WebRequest.Create(requestUri.ToString().ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled));
                }

                foreach (var item in behaviors.DeviceInfo)
                    webRequest.Headers.Add(item.Key, item.Value);

                webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                webRequest.Timeout = 5000;

                // Set the method type
                //webRequest.Timeout = 10;
                //webRequest.ReadWriteTimeout = 10;
                webRequest.Method = "POST";
                webRequest.Accept = ApplicationContext.Current.Settings.BitMobileFormatterDisabled ? "application/atom+xml" : "application/bitmobile";
                webRequest.ContentType = ApplicationContext.Current.Settings.BitMobileFormatterDisabled ? "application/atom+xml" : "application/bitmobile";

                wrapper.WebRequest = webRequest;

                var requestData = new TimeoutRequestData()
                {
                    Request = wrapper.WebRequest
                };

                // Get the request stream
                if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                {
                    lock (_timeoutSync)
                    {
                        IAsyncResult resultUpload = webRequest.BeginGetRequestStream(OnUploadGetRequestStreamCompleted, wrapper);
                        requestData.Handle = ThreadPool.RegisterWaitForSingleObject(resultUpload.AsyncWaitHandle,
                            new WaitOrTimerCallback(TimeOutCallback), requestData, TIMEOUT, true);
                    }
                }
                else
                {
                    lock (_timeoutSync)
                    {
                        IAsyncResult resultDownload = webRequest.BeginGetRequestStream(OnDownloadGetRequestStreamCompleted, wrapper);
                        requestData.Handle = ThreadPool.RegisterWaitForSingleObject(resultDownload.AsyncWaitHandle,
                            new WaitOrTimerCallback(TimeOutCallback), requestData, TIMEOUT, true);
                    }
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        /// <summary>
        /// Callback for the Upload HttpWebRequest.beginGetRequestStream
        /// </summary>
        /// <param name="asyncResult">IAsyncResult object</param>
        void OnUploadGetRequestStreamCompleted(IAsyncResult asyncResult)
        {
            AsyncArgsWrapper wrapper = asyncResult.AsyncState as AsyncArgsWrapper;
            try
            {
                Stream requestStream = wrapper.WebRequest.EndGetRequestStream(asyncResult);

                // CreateInstance a SyncWriter to write the contents

                if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                    _syncWriter = new ODataAtomWriter(BaseUri);
                else
                    _syncWriter = new BMWriter(BaseUri);

                this._syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

                foreach (IOfflineEntity entity in wrapper.CacheRequest.Changes)
                {
                    var ientity = entity as IEntity;

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
                        if (wrapper.TempIdToEntityMapping == null)
                        {
                            wrapper.TempIdToEntityMapping = new Dictionary<string, IOfflineEntity>();
                        }
                        tempId = Guid.NewGuid().ToString();
                        wrapper.TempIdToEntityMapping.Add(tempId, entity);
                    }

                    this._syncWriter.AddItem(entity, tempId);

                    if (ientity != null)
                        LogManager.Logger.SyncUpload(ientity.EntityType);
                }

                if (base.SerializationFormat == SerializationFormat.ODataAtom)
                {
                    this._syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                }
                else
                {
                    this._syncWriter.WriteFeed(JsonReaderWriterFactory.CreateJsonWriter(requestStream));
                }

                requestStream.Flush();
                requestStream.Close();

                if (this._beforeRequestHandler != null)
                {
                    // Invoke user code and wait for them to call back us when they are done with the input request
                    this._workerManager.PostProgress(wrapper.WorkerRequest, this.FirePreRequestHandler, wrapper);
                }
                else
                {
                    this.GetWebResponse(wrapper);
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        /// <summary>
        /// Callback for the Download HttpWebRequest.beginGetRequestStream
        /// </summary>
        /// <param name="asyncResult">IAsyncResult object</param>
        void OnDownloadGetRequestStreamCompleted(IAsyncResult asyncResult)
        {
            AsyncArgsWrapper wrapper = asyncResult.AsyncState as AsyncArgsWrapper;
            try
            {
                Stream requestStream = wrapper.WebRequest.EndGetRequestStream(asyncResult);

                // CreateInstance a SyncWriter to write the contents
                if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                    _syncWriter = new ODataAtomWriter(BaseUri);
                else
                    _syncWriter = new BMWriter(BaseUri);


                this._syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

                if (base.SerializationFormat == SerializationFormat.ODataAtom)
                {
                    this._syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                }
                else
                {
                    this._syncWriter.WriteFeed(JsonReaderWriterFactory.CreateJsonWriter(requestStream));
                }

                requestStream.Flush();
                requestStream.Close();

                if (this._beforeRequestHandler != null)
                {
                    // Invoke user code and wait for them to call back us when they are done with the input request
                    this._workerManager.PostProgress(wrapper.WorkerRequest, this.FirePreRequestHandler, wrapper);
                }
                else
                {
                    this.GetWebResponse(wrapper);
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        /// <summary>
        /// Invokes the user BeforeSendingRequest callback. It also passes the resumption handler for the user
        /// to call when they are done with the customizations on the request.
        /// </summary>
        /// <param name="state">Async user state object. Ignored.</param>
        void FirePreRequestHandler(object state)
        {
            AsyncArgsWrapper wrapper = state as AsyncArgsWrapper;
            try
            {
                // Add this to the requestToArgs mapper so we can look up the args when the user calls ResumeRequestProcessing
                this._requestToArgsMapper[wrapper.WebRequest.GetHashCode()] = wrapper;

                // Invoke the user code.
                this._beforeRequestHandler(wrapper.WebRequest, ResumeRequestProcessing);
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        /// <summary>
        /// Invokes the user's AfterReceivingResponse callback.
        /// </summary>
        /// <param name="wrapper">AsyncArgsWrapper object</param>
        void FirePostResponseHandler(AsyncArgsWrapper wrapper)
        {
            if (this._afterResponseHandler != null)
            {
                // Invoke the user code.
                this._afterResponseHandler(wrapper.WebResponse);
            }
        }

        /// <summary>
        /// Handler that the user will call when they want the request to resume processing.
        /// It will check to ensure that the correct WebRequest is passed back to this resumption point.
        /// Else an error will be thrown.
        /// </summary>
        /// <param name="request">HttpWebRequest for which the processing has to resume.</param>
        void ResumeRequestProcessing(HttpWebRequest request)
        {
            AsyncArgsWrapper wrapper = null;

            this._requestToArgsMapper.TryGetValue(request.GetHashCode(), out wrapper);
            if (wrapper == null)
            {
                // It means they called Resume with another WebRequest. Fail sync.
                throw new CacheControllerException("Incorrect HttpWebRequest object passed to ResumeRequestProcessing callback.");
            }

            try
            {
                this._requestToArgsMapper.Remove(request.GetHashCode());

                this.GetWebResponse(wrapper);
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }

                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        /// <summary>
        /// Issues the BeginGetResponse call for the HttpWebRequest
        /// </summary>
        /// <param name="wrapper">AsyncArgsWrapper object</param>
        private void GetWebResponse(AsyncArgsWrapper wrapper)
        {

            var requestData = new TimeoutRequestData()
            {
                Request = wrapper.WebRequest
            };
            // Send the request and wait for the response.
            if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
            {
                lock (_timeoutSync)
                {
                    IAsyncResult resultUpload = wrapper.WebRequest.BeginGetResponse(OnUploadGetResponseCompleted, wrapper);
                    requestData.Handle = ThreadPool.RegisterWaitForSingleObject(resultUpload.AsyncWaitHandle,
                        new WaitOrTimerCallback(TimeOutCallback), requestData, TIMEOUT, true);
                }
            }
            else
            {
                lock (_timeoutSync)
                {
                    IAsyncResult resultDownload = wrapper.WebRequest.BeginGetResponse(OnDownloadGetResponseCompleted, wrapper);
                    requestData.Handle = ThreadPool.RegisterWaitForSingleObject(resultDownload.AsyncWaitHandle,
                        new WaitOrTimerCallback(TimeOutCallback), requestData, TIMEOUT, true);
                }
            }
        }

        private void TimeOutCallback(object state, bool timedOut)
        {
            var requestData = (TimeoutRequestData)state;
            if (timedOut)
            {
                HttpWebRequest request = requestData.Request;
                if (request != null)
                    request.Abort();
            }

            lock (_timeoutSync)
            {
                requestData.Handle.Unregister(null);
            }
        }

        class TimeoutRequestData
        {
            public HttpWebRequest Request;
            public RegisteredWaitHandle Handle;
        }

        /// <summary>
        /// Callback for the Upload HttpWebRequest.BeginGetResponse call
        /// </summary>
        /// <param name="asyncResult">IAsyncResult object</param>
        void OnUploadGetResponseCompleted(IAsyncResult asyncResult)
        {
            AsyncArgsWrapper wrapper = asyncResult.AsyncState as AsyncArgsWrapper;

            wrapper.UploadResponse = new ChangeSetResponse();

            HttpWebResponse response = null;
            try
            {
                try
                {
                    response = wrapper.WebRequest.EndGetResponse(asyncResult) as HttpWebResponse;
                }
                catch (WebException we)
                {
                    wrapper.UploadResponse.Error = we;
                    // If we get here then it means we completed the request. Return to the original caller
                    this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
                    return;
                }
                catch (SecurityException se)
                {
                    wrapper.UploadResponse.Error = se;
                    // If we get here then it means we completed the request. Return to the original caller
                    this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();

                    // CreateInstance the SyncReader
                    if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                        _syncReader = new ODataAtomReader(responseStream, _knownTypes);
                    else
                        _syncReader = new BMReader(responseStream, _knownTypes);

                    // Read the response
                    while (this._syncReader.Next())
                    {
                        switch (this._syncReader.ItemType)
                        {
                            case ReaderItemType.Entry:
                                IOfflineEntity entity = this._syncReader.GetItem();
                                IOfflineEntity ackedEntity = entity;
                                string tempId = null;

                                // If conflict only one temp ID should be set
                                if (this._syncReader.HasTempId() && this._syncReader.HasConflictTempId())
                                {
                                    throw new CacheControllerException(string.Format("Service returned a TempId '{0}' in both live and conflicting entities.",
                                                                                       this._syncReader.GetTempId()));
                                }

                                // Validate the live temp ID if any, before adding anything to the offline context
                                if (this._syncReader.HasTempId())
                                {
                                    tempId = this._syncReader.GetTempId();
                                    CheckEntityServiceMetadataAndTempIds(wrapper, entity, tempId);

                                }

                                //  If conflict 
                                if (this._syncReader.HasConflict())
                                {
                                    Conflict conflict = this._syncReader.GetConflict();
                                    IOfflineEntity conflictEntity = (conflict is SyncConflict) ?
                                                                        ((SyncConflict)conflict).LosingEntity : ((SyncError)conflict).ErrorEntity;

                                    // Validate conflict temp ID if any
                                    if (this._syncReader.HasConflictTempId())
                                    {
                                        tempId = this._syncReader.GetConflictTempId();
                                        CheckEntityServiceMetadataAndTempIds(wrapper, conflictEntity, tempId);
                                    }

                                    // Add conflict                                    
                                    wrapper.UploadResponse.AddConflict(conflict);

                                    //
                                    // If there is a conflict and the tempId is set in the conflict entity then the client version lost the 
                                    // conflict and the live entity is the server version (ServerWins)
                                    //
                                    if (this._syncReader.HasConflictTempId() && entity.ServiceMetadata.IsTombstone)
                                    {
                                        //
                                        // This is a ServerWins conflict, or conflict error. The winning version is a tombstone without temp Id
                                        // so there is no way to map the winning entity with a temp Id. The temp Id is in the conflict so we are
                                        // using the conflict entity, which has the PK, to build a tombstone entity used to update the offline context
                                        //
                                        // In theory, we should copy the service metadata but it is the same end result as the service fills in
                                        // all the properties in the conflict entity
                                        //

                                        // Add the conflict entity                                              
                                        conflictEntity.ServiceMetadata.IsTombstone = true;
                                        ackedEntity = conflictEntity;
                                    }
                                }

                                // Add ackedEntity to storage. If ackedEntity is still equal to entity then add non-conflict entity. 
                                if (!String.IsNullOrEmpty(tempId))
                                {
                                    wrapper.UploadResponse.AddUpdatedItem(ackedEntity);
                                }
                                break;

                            case ReaderItemType.SyncBlob:
                                wrapper.UploadResponse.ServerBlob = this._syncReader.GetServerBlob();
                                break;
                        }
                    }

                    wrapper.WebResponse = response;
                    // Invoke user code on the correct synchronization context.
                    this.FirePostResponseHandler(wrapper);
                }
                else
                {
                    wrapper.UploadResponse.Error = new CacheControllerException(
                        string.Format("Remote service returned error status. Status: {0}, Description: {1}",
                                       response.StatusCode,
                                       response.StatusDescription));
                }

                // If we get here then it means we completed the request. Return to the original caller
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }

                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        private void CheckEntityServiceMetadataAndTempIds(AsyncArgsWrapper wrapper, IOfflineEntity entity, string tempId)
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
            if (!wrapper.TempIdToEntityMapping.ContainsKey(tempId))
            {
                throw new CacheControllerException("Service returned a response for a tempId which was not uploaded by the client. TempId: " + tempId);
            }

            // Once received, remove the tempId from the mapping list.
            wrapper.TempIdToEntityMapping.Remove(tempId);
        }

        /// <summary>
        /// Callback for the Download HttpWebRequest.beginGetRequestStream. Deserializes the response feed to
        /// retrieve the list of IOfflineEntity objects and constructs an ChangeSet for that.
        /// </summary>
        /// <param name="asyncResult">IAsyncResult object</param>
        void OnDownloadGetResponseCompleted(IAsyncResult asyncResult)
        {
            AsyncArgsWrapper wrapper = asyncResult.AsyncState as AsyncArgsWrapper;

            wrapper.DownloadResponse = new ChangeSet();

            HttpWebResponse response = null;
            try
            {
                try
                {
                    response = wrapper.WebRequest.EndGetResponse(asyncResult) as HttpWebResponse;
                    if (String.IsNullOrEmpty(behaviors.UserId))
                        behaviors.UserId = response.Headers["userid"];
                    if (string.IsNullOrWhiteSpace(behaviors.UserEmail))
                        behaviors.UserEmail = response.Headers["email"];
                    behaviors.ResourceVersion = response.Headers["resourceversion"];
                }
                catch (WebException we)
                {
                    wrapper.Error = we;
                    // If we get here then it means we completed the request. Return to the original caller
                    this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
                    return;
                }
                catch (SecurityException se)
                {
                    wrapper.Error = se;
                    // If we get here then it means we completed the request. Return to the original caller
                    this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    behaviors.SaveUserSession();

                    int contentLength = (int)response.ContentLength;

                    if (response.Headers.AllKeys.Contains("unzippedcontentlength"))
                    {
                        string value = response.Headers["unzippedcontentlength"];
                        if (!int.TryParse(value, out contentLength))
                            throw new WebException("Invalid value of header unzippedcontentlength: " + value);
                    }
                    
                    Stream responseStream = new ProgressStream(response.GetResponseStream()
                    , contentLength
                    , behaviors.ReadProgressCallback);

                    // CreateInstance the SyncReader
                    if (ApplicationContext.Current.Settings.BitMobileFormatterDisabled)
                        _syncReader = new ODataAtomReader(responseStream, _knownTypes);
                    else
                        _syncReader = new BMReader(responseStream, _knownTypes);

                    // Read the response
                    wrapper.DownloadResponse.Data = GetDownloadedValues(wrapper);

                    wrapper.WebResponse = response;
                    // Invoke user code on the correct synchronization context.
                    this.FirePostResponseHandler(wrapper);
                }
                else
                {
                    wrapper.Error = new CacheControllerException(
                        string.Format("Remote service returned error status. Status: {0}, Description: {1}",
                                       response.StatusCode,
                                       response.StatusDescription));
                }

                // If we get here then it means we completed the request. Return to the original caller
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }

                wrapper.Error = e;
                this._workerManager.CompleteWorkRequest(wrapper.WorkerRequest, wrapper);
            }
        }

        private IEnumerable<IsolatedStorageOfflineEntity> GetDownloadedValues(AsyncArgsWrapper wrapper)
        {
            while (this._syncReader.Next())
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
                        wrapper.DownloadResponse.ServerBlob = _syncReader.GetServerBlob();
                        break;
                    case ReaderItemType.HasMoreChanges:
                        wrapper.DownloadResponse.IsLastBatch = !_syncReader.GetHasMoreChangesValue();
                        break;
                }
            }
            _syncReader.Dispose();
        }

        /// <summary>
        /// Wrapper class that holds multiple related arguments that is passed around from
        /// async call to its completion
        /// </summary>
        class AsyncArgsWrapper
        {
            public CacheRequest CacheRequest;
            public AsyncWorkRequest WorkerRequest;
            public object UserPassedState;
            public ChangeSetResponse UploadResponse;
            public ChangeSet DownloadResponse;
            public Exception Error;
            public HttpWebRequest WebRequest;
            public HttpWebResponse WebResponse;
            public Dictionary<string, IOfflineEntity> TempIdToEntityMapping;
        }
    }
}
