using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization.Json;
using MonoTouch.Foundation;
using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.Services.Formatters;
using System.Linq;
using System.Net;
using MonoTouch.UIKit;
using BitMobile.Common.Entites;

namespace Microsoft.Synchronization.ClientServices
{
	sealed class NSUrlCacheRequestHandler: CacheRequestHandler, IDisposable
	{
		const int TIMEOUT = 10 * 60;

		object _lockObject;
		AsyncWorkerManager _workerManager;
		SyncReader _syncReader;
		SyncWriter _syncWriter;
		EntityType[] _knownTypes;
		AsyncArgsWrapper _wrapper;
		ICredentials _credentials;
		Dictionary<string, string> _scopeParameters;
		CacheControllerBehavior _behaviors;
		NSUrlSessionDownloadTask _currentTask;
		NSUrlSession _downloadSession;
		NSUrlSession _uploadSession;
		bool _disposed = false;

		public NSUrlCacheRequestHandler (Uri serviceUri, CacheControllerBehavior behaviors, AsyncWorkerManager manager)
			: base (serviceUri, behaviors.SerializationFormat, behaviors.ScopeName)
		{
			_workerManager = manager;
			_scopeParameters = new Dictionary<string, string> (behaviors.ScopeParametersInternal);
			_knownTypes = new EntityType[behaviors.KnownTypes.Count];
			behaviors.KnownTypes.CopyTo (this._knownTypes, 0);
			_lockObject = new object ();

			_credentials = behaviors.Credentials;
			_behaviors = behaviors;
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		void Dispose (bool disposing)
		{
			if (!_disposed) {
				lock (_lockObject) {
					if (_downloadSession != null) {
						_downloadSession.Dispose ();
						_downloadSession = null;
					}

					if (_uploadSession != null) {
						_uploadSession.Dispose ();
						_uploadSession = null;
					}
				}
			}

			_disposed = true;
		}

		~NSUrlCacheRequestHandler ()
		{
			Dispose (false);
		}

		#endregion

		public override void ProcessCacheRequestAsync (CacheRequest request, object state)
		{
			_workerManager.AddWorkRequest (new AsyncWorkRequest (ProcessCacheRequestWorker, CacheRequestCompleted, request, state));
		}

		void ProcessCacheRequestWorker (AsyncWorkRequest worker, object[] inputParams)
		{
			CacheRequest request = inputParams [0] as CacheRequest;
			object state = inputParams [1];

			_wrapper = new AsyncArgsWrapper () {
				UserPassedState = state,
				WorkerRequest = worker,
				CacheRequest = request
			};

			ProcessRequest ();
		}

		void ProcessRequest ()
		{
			try {
				StringBuilder requestUri = new StringBuilder ();
				requestUri.AppendFormat ("{0}{1}{2}/{3}",
					base.BaseUri,
					(base.BaseUri.ToString ().EndsWith ("/")) ? string.Empty : "/",
					Uri.EscapeUriString (base.ScopeName),
					_wrapper.CacheRequest.RequestType.ToString ());

				string prefix = "?";

				// Add the scope params if any
				foreach (KeyValuePair<string, string> kvp in _scopeParameters) {
					requestUri.AppendFormat ("{0}{1}={2}", prefix, Uri.EscapeUriString (kvp.Key), Uri.EscapeUriString (kvp.Value));
					if (prefix.Equals ("?")) {
						prefix = "&";
					}
				}

				// Create the WebRequest
				NSMutableUrlRequest webRequest = new NSMutableUrlRequest (new NSUrl (requestUri.ToString ()));
				if (this._credentials != null) {
					NetworkCredential credential = this._credentials.GetCredential (BaseUri, "Basic");
					string svcCredentials = Convert.ToBase64String (ASCIIEncoding.ASCII.GetBytes (credential.UserName + ":" + credential.Password));
					webRequest ["Authorization"] = "Basic " + svcCredentials;

					webRequest ["configname"] = _behaviors.ConfigName;
					webRequest ["configversion"] = _behaviors.ConfigVersion;

					webRequest ["coreversion"] = _behaviors.CoreVersion.ToString ();
				} else
					throw new Exception ("Credentials is null");
			
				foreach (var item in _behaviors.DeviceInfo)
					webRequest [item.Key] = item.Value;

				webRequest.HttpMethod = "POST";
				webRequest ["Accept"] = (base.SerializationFormat == SerializationFormat.ODataAtom) ? "application/atom+xml" : "application/json";
				webRequest ["Content-Type"] = (base.SerializationFormat == SerializationFormat.ODataAtom) ? "application/atom+xml" : "application/json";
				webRequest ["Accept-Encoding"] = "gzip, deflate";
				webRequest.TimeoutInterval = TIMEOUT;

				webRequest.Body = CreateRequestBody ();

				_wrapper.WebRequest = webRequest;

				if (_wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges) {
					lock (_lockObject) {
						_currentTask = CreateUploadSession ().CreateDownloadTask (webRequest);
						_currentTask.Resume ();
					}
				} else {
					lock (_lockObject) {
						_currentTask = CreateDownloadSession ().CreateDownloadTask (webRequest);
						_currentTask.Resume ();
					}
				}
			} catch (Exception e) {
				if (ExceptionUtility.IsFatal (e))
					throw;
				
				_wrapper.Error = e;

				_workerManager.CompleteWorkRequest (_wrapper.WorkerRequest, _wrapper);
			}
		}

		NSUrlSession CreateDownloadSession ()
		{
			if (_downloadSession == null) {
				string urlSessioinId = Guid.NewGuid ().ToString ();
				NSUrlSessionConfiguration sessionConfiguration;
				if (new Version (UIDevice.CurrentDevice.SystemVersion).Major > 7)
					sessionConfiguration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (urlSessioinId);
				else
					sessionConfiguration = NSUrlSessionConfiguration.BackgroundSessionConfiguration (urlSessioinId);
				sessionConfiguration.TimeoutIntervalForRequest = TIMEOUT;
				sessionConfiguration.TimeoutIntervalForResource = TIMEOUT;

				NSUrlDownloadDelegate downloadDelegate = new NSUrlDownloadDelegate (OnDownloadCompleted, OnProgress);

				_downloadSession = NSUrlSession.FromConfiguration (sessionConfiguration, downloadDelegate, new NSOperationQueue ());

			}

			return _downloadSession;		
		}

		NSUrlSession CreateUploadSession ()
		{
			if (_uploadSession == null) {
				string urlSessioinId = Guid.NewGuid ().ToString ();
				NSUrlSessionConfiguration sessionConfiguration;
				if (new Version (UIDevice.CurrentDevice.SystemVersion).Major > 7)
					sessionConfiguration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (urlSessioinId);
				else
					sessionConfiguration = NSUrlSessionConfiguration.BackgroundSessionConfiguration (urlSessioinId);
				sessionConfiguration.TimeoutIntervalForRequest = TIMEOUT;
				sessionConfiguration.TimeoutIntervalForResource = TIMEOUT;

				NSUrlUploadDelegate uploadDelegate = new NSUrlUploadDelegate (OnUploadCompleted, _behaviors.ReadProgressCallback);

				_uploadSession = NSUrlSession.FromConfiguration (sessionConfiguration, uploadDelegate, new NSOperationQueue ());
			}

			return _uploadSession;
		}

		void OnProgress (int totalBytes, int processed)
		{
			NSDictionary headers = ((NSHttpUrlResponse)_currentTask.Response).AllHeaderFields;
			int total;
			NSObject lengthHeader = headers ["unzippedcontentlength"];
			if (lengthHeader != null && int.TryParse (lengthHeader.ToString (), out total)) {
				totalBytes = total;
			}
			_behaviors.ReadProgressCallback (totalBytes, processed);
		}

		NSData CreateRequestBody ()
		{
			_syncWriter = (SyncWriter)new ODataAtomWriter (base.BaseUri);

			_syncWriter.StartFeed (_wrapper.CacheRequest.IsLastBatch, _wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

			MemoryStream requestStream = new MemoryStream ();

			if (_wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges) {
				foreach (IOfflineEntity entity in _wrapper.CacheRequest.Changes) {
					// Skip tombstones that dont have a ID element.
					if (entity.ServiceMetadata.IsTombstone && string.IsNullOrEmpty (entity.ServiceMetadata.Id)) {
						continue;
					}

					string tempId = null;

					// Check to see if this is an insert. i.e ServiceMetadata.Id is null or empty
					if (string.IsNullOrEmpty (entity.ServiceMetadata.Id)) {
						if (_wrapper.TempIdToEntityMapping == null) {
							_wrapper.TempIdToEntityMapping = new Dictionary<string, IOfflineEntity> ();
						}

						tempId = Guid.NewGuid ().ToString ();

						_wrapper.TempIdToEntityMapping.Add (tempId, entity);
					}

					_syncWriter.AddItem (entity, tempId);
				}
			}

			if (base.SerializationFormat == SerializationFormat.ODataAtom) {
				_syncWriter.WriteFeed (XmlWriter.Create (requestStream));
			} else {
				_syncWriter.WriteFeed (JsonReaderWriterFactory.CreateJsonWriter (requestStream));
			}

			string result = "";

			requestStream.Seek (0, SeekOrigin.Begin);

			using (StreamReader reader = new StreamReader (requestStream)) {
				result = reader.ReadToEnd ();
			}

			requestStream.Flush ();
			requestStream.Close ();

			NSString dataString = new NSString (result);

			return dataString.DataUsingEncoding (NSStringEncoding.UTF8);
		}

		void CacheRequestCompleted (object state)
		{
			// Fire the ProcessCacheRequestCompleted handler
			AsyncArgsWrapper wrapper = state as AsyncArgsWrapper;
			if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges) {
				base.OnProcessCacheRequestCompleted (
					new ProcessCacheRequestCompletedEventArgs (
						wrapper.CacheRequest.RequestId,
						wrapper.UploadResponse,
						wrapper.CacheRequest.Changes.Count,
						wrapper.Error,
						wrapper.UserPassedState)
				);
			} else {
				base.OnProcessCacheRequestCompleted (
					new ProcessCacheRequestCompletedEventArgs (
						wrapper.CacheRequest.RequestId,
						wrapper.DownloadResponse,
						wrapper.Error,
						wrapper.UserPassedState)
				);
			}
		}

		void OnUploadCompleted (object sender, NSUrlEventArgs e)
		{
			_wrapper.UploadResponse = new ChangeSetResponse ();

			FileStream fileStream = null;

			string filePath = null;

			try {
				if (e.Error == null) {
					string responseDescription = "response is  null";
					NSHttpUrlResponse response = (NSHttpUrlResponse)_currentTask.Response;
				if (response != null)
						responseDescription = response.Description;

					filePath = e.FilePath.Replace ("file://", "").Replace ("%20", " ");
					if (File.Exists (filePath)) {
						fileStream = File.OpenRead (filePath);
						fileStream.Seek (0, SeekOrigin.Begin);

						// Create the SyncReader
						_syncReader = (SyncReader)new ODataAtomReader (fileStream, _knownTypes);

						// Read the response
						while (_syncReader.Next ()) {
							switch (_syncReader.ItemType) {
							case ReaderItemType.Entry:                                
								IOfflineEntity entity = _syncReader.GetItem ();
								IOfflineEntity ackedEntity = entity;                                
								string tempId = null;

								if (_syncReader.HasTempId () && _syncReader.HasConflictTempId ()) {
									throw new CacheControllerException (string.Format ("Service returned a TempId '{0}' in both live and conflicting entities.", 
										_syncReader.GetTempId ()));
								}

								if (_syncReader.HasTempId ()) {
									tempId = _syncReader.GetTempId ();
									CheckEntityServiceMetadataAndTempIds (entity, tempId);
								}

								if (_syncReader.HasConflict ()) {                                    
									Conflict conflict = _syncReader.GetConflict ();
									IOfflineEntity conflictEntity = (conflict is SyncConflict) ? 
									                                ((SyncConflict)conflict).LosingEntity : ((SyncError)conflict).ErrorEntity;

									if (this._syncReader.HasConflictTempId ()) {
										tempId = _syncReader.GetConflictTempId ();
										CheckEntityServiceMetadataAndTempIds (conflictEntity, tempId);
									}
									                           
									_wrapper.UploadResponse.AddConflict (conflict);

									if (_syncReader.HasConflictTempId () && entity.ServiceMetadata.IsTombstone) {                                        
										conflictEntity.ServiceMetadata.IsTombstone = true;
										ackedEntity = conflictEntity;
									}
								}

								if (!String.IsNullOrEmpty (tempId)) {
									_wrapper.UploadResponse.AddUpdatedItem (ackedEntity);
								}
								                       
								break;

							case ReaderItemType.SyncBlob:
								_wrapper.UploadResponse.ServerBlob = _syncReader.GetServerBlob ();
								break;
							}
						}

						if (_wrapper.TempIdToEntityMapping != null && _wrapper.TempIdToEntityMapping.Count != 0) {
							StringBuilder builder = new StringBuilder ("Server did not acknowledge with a permanent Id for the following tempId's: ");
							builder.Append (string.Join (",", _wrapper.TempIdToEntityMapping.Keys.ToArray ()));
							throw new CacheControllerException (builder.ToString ());
						}
					} else
						_wrapper.Error = new FileNotFoundException (String.Format ("Downloaded data file not found! {0}, Description: {1}", e.FilePath, responseDescription));
				} else
				{
					NSHttpUrlResponse response = _currentTask.Response as NSHttpUrlResponse;
					HandleError (e.Error, response);
				}

				_workerManager.CompleteWorkRequest (_wrapper.WorkerRequest, _wrapper);
			} catch (Exception ex) {
				if (ExceptionUtility.IsFatal (ex)) {
					throw ex;
				}

				_wrapper.Error = ex;

				_workerManager.CompleteWorkRequest (_wrapper.WorkerRequest, _wrapper);
			} finally {
				if (fileStream != null) {
					fileStream.Close ();
				}

				if (filePath != null && File.Exists (filePath)) {
					File.Delete (filePath);
				}
			}
		}

		void OnDownloadCompleted (object sender, NSUrlEventArgs e)
		{
			FileStream fileStream = null;
			string filePath = null;

			try {
				if (e.Error == null) {
					filePath = e.FilePath.Replace ("file://", "").Replace ("%20", " ");

					NSHttpUrlResponse response = (NSHttpUrlResponse)_currentTask.Response;
					if (response != null) {

						HttpStatusCode code = (HttpStatusCode)response.StatusCode; 
						if (code == HttpStatusCode.OK) {

							NSDictionary headers = response.AllHeaderFields;
							if (string.IsNullOrWhiteSpace (_behaviors.UserId))
								_behaviors.UserId = headers ["userid"].ToString ();
							if (string.IsNullOrWhiteSpace (_behaviors.UserEmail))
								_behaviors.UserEmail = headers ["email"].ToString ();
							;
							_behaviors.SaveUserSession ();

							if (File.Exists (filePath)) {
								fileStream = File.OpenRead (filePath);

								fileStream.Seek (0, SeekOrigin.Begin);
								int contentLength;
								if (!int.TryParse (headers ["unzippedcontentlength"].ToString (), out contentLength))
									contentLength = -1;					
								Stream responseStream = new ProgressStream (fileStream, contentLength, _behaviors.ReadProgressCallback);

								// Create the SyncReader
								_syncReader = (SyncReader)new ODataAtomReader (responseStream, _knownTypes);

								_wrapper.DownloadResponse = new ChangeSet ();

								// Read the response
								while (this._syncReader.Next ()) {
									switch (this._syncReader.ItemType) {
									case ReaderItemType.Entry:
										_wrapper.DownloadResponse.AddItem (_syncReader.GetItem ());
										break;
									case ReaderItemType.SyncBlob:
										_wrapper.DownloadResponse.ServerBlob = _syncReader.GetServerBlob ();
										break;
									case ReaderItemType.HasMoreChanges:
										_wrapper.DownloadResponse.IsLastBatch = !_syncReader.GetHasMoreChangesValue ();
										break;
									}
								}	
							} else {
								_wrapper.Error = new FileNotFoundException (String.Format ("Downloaded data file not found! {0}, Description: {1}", e.FilePath, response.Description));
							}
						} else
							_wrapper.Error = new CacheControllerWebException (string.Format ("Remote service returned error status. Status: {0}, Description: {1}", code, response.Description), code);
					} else
						_wrapper.Error = new CacheControllerException ("Response is null");
				} else {
					NSHttpUrlResponse response = _currentTask.Response as NSHttpUrlResponse;
					HandleError (e.Error, response);
				}

				// If we get here then it means we completed the request. Return to the original caller
				_workerManager.CompleteWorkRequest (_wrapper.WorkerRequest, _wrapper);
			
			} catch (Exception ex) {
				if (ExceptionUtility.IsFatal (ex)) {
					throw;
				}

				_wrapper.Error = ex;

				_workerManager.CompleteWorkRequest (_wrapper.WorkerRequest, _wrapper);
			} finally {
				if (fileStream != null) {
					fileStream.Close ();
				}

				if (filePath != null && File.Exists (filePath)) {
					File.Delete (filePath);
				}
			}
		}

		void HandleError (string error, NSHttpUrlResponse response)
		{
			if (response != null) {
				string msg = string.Format ("Remote service returned error {0}. Status: {1}, Description: {2}", error, response.StatusCode, response.Description);
				_wrapper.Error = new CacheControllerWebException (msg, (HttpStatusCode)response.StatusCode);
			} else
				_wrapper.Error = new CacheControllerException (error);
		}

		void CheckEntityServiceMetadataAndTempIds (IOfflineEntity entity, string tempId)
		{
			// Check service ID 
			if (string.IsNullOrEmpty (entity.ServiceMetadata.Id)) {
				throw new CacheControllerException (string.Format ("Service did not return a permanent Id for tempId '{0}'", tempId));
			}

			// If an entity has a temp id then it should not be a tombstone                
			if (entity.ServiceMetadata.IsTombstone) {
				throw new CacheControllerException (string.Format ("Service returned a tempId '{0}' in tombstoned entity.", tempId));
			}

			// Check that the tempId was sent by client
			if (!_wrapper.TempIdToEntityMapping.ContainsKey (tempId)) {
				throw new CacheControllerException ("Service returned a response for a tempId which was not uploaded by the client. TempId: " + tempId);
			}

			// Once received, remove the tempId from the mapping list.
			_wrapper.TempIdToEntityMapping.Remove (tempId);            
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

