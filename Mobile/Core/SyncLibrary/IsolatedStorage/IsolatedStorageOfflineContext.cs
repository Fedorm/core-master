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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.IO.IsolatedStorage;
using System.IO;
using System.Security.Cryptography;
using BitMobile.Common.Entites;
using BitMobile.SyncLibrary.IsolatedStorage;

[assembly: InternalsVisibleTo("System.ServiceModel.Web, PublicKey=00240000048000009400000006020000002400005253413100040000010001008d56c76f9e8649383049f383c44be0ec204181822a6c31cf5eb7ef486944d032188ea1d3920763712ccb12d75fb77e9811149e6148e5d32fbaab37611c1878ddc19e20ef135d0cb2cff2bfec3d115810c3d9069638fe4be215dbf795861920e5ab6f7db2e2ceef136ac23d5dd2bf031700aec232f6c6b1c785b4305c123b37ab")]
namespace Microsoft.Synchronization.ClientServices.IsolatedStorage
{
    /// <summary>
    /// IsolatedStorageOfflineContext
    /// </summary>
    [Serializable]
    public class IsolatedStorageOfflineContext : OfflineSyncProvider, IDisposable
    {
        /// <summary>
        /// Constructor for the offline context.
        /// </summary>
        /// <param name="schema">The schema that specifies the set of the collections for the context.</param>
        /// <param name="scopeName">The scope name used to identify the scope on the service.</param>
        /// <param name="cachePath">Path in isolated storage where the data will be stored.</param>
        /// <param name="uri">Uri of the scopeName.  Used to intialize the CacheController.</param>
        /// <remarks>
        /// If the Uri specified is different from the one that is stored in the cache path, the
        /// Load method will throw an InvalidOperationException.
        /// </remarks>
        public IsolatedStorageOfflineContext(IsolatedStorageSchema schema, string scopeName, string cachePath,
            Uri uri) : this(schema, scopeName, cachePath, uri, null)
        {
        }

        
        /// <summary>
        /// Constructor for the offline context which allows a symmetric encryption algorithm to be specified.
        /// </summary>
        /// <param name="schema">The schema that specifies the set of the collections for the context.</param>
        /// <param name="scopeName">The scope name used to identify the scope on the service.</param>
        /// <param name="cachePath">Path in isolated storage where the data will be stored.</param>
        /// <param name="uri">Uri of the scopeName.  Used to intialize the CacheController.</param>
        /// <param name="encryptionAlgorithm">The symmetric encryption algorithm to use for files on disk</param>
        /// <remarks>
        /// If the Uri specified is different from the one that is stored in the cache path, the
        /// Load method will throw an InvalidOperationException.
        /// </remarks>
        public IsolatedStorageOfflineContext(IsolatedStorageSchema schema, string scopeName, string cachePath,
            Uri uri, SymmetricAlgorithm encryptionAlgorithm)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }

            if (string.IsNullOrEmpty(scopeName))
            {
                throw new ArgumentNullException("scopeName");
            }

            if (string.IsNullOrEmpty(cachePath))
            {
                throw new ArgumentNullException("cachePath");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            _isDisposed = false;

            _schema = schema;
            _scopeUri = uri;
            _scopeName = scopeName;
            _cachePath = cachePath;
            _storageHandler = new SQLiteStorageHandler(this, schema, cachePath, encryptionAlgorithm);
            _saveSyncLock = new AutoResetLock();
            
            CreateCacheController();
        }

        /// <summary>
        /// Explicitly loads the data from the cache into memory synchronously.
        /// </summary>
        /// <remarks>
        /// Performing any method on the cache will implicitly load it if this method is not called. This
        /// method allows better control over when data is loaded.
        /// If the cache is already loaded, this method will do nothing.
        /// </remarks>
        public void Load()
        {
            ThrowIfDisposed();

            // Double-checked lock to ensure noone else is loading.
            if (!_loaded)
            {
                // Block in case anyone else is loading
                lock (_loadLock)
                {
                    // Only continue if we still aren't loaded
                    if (!_loaded)
                    {
                        // Do the actual loading.
                        LoadInternal();
                    }
                }
            }
        }

        /// <summary>
        /// Loads the data from the cache into memory asynchronously.
        /// </summary>
        /// <remarks>
        /// Performing any method on the cache will implicitly load it if this method is not called. This
        /// method allows better control over when data is loaded.
        /// If the cache is already loaded, this method will do nothing.
        /// </remarks>        
        public void LoadAsync()
        {
            ThrowIfDisposed();

            // Use the ThreadPool to queue our load.  This will happen regardless of whether the cache is already loaded
            ThreadPool.QueueUserWorkItem(LoadThread);
        }
        
        /// <summary>
        /// Event called when the LoadAsync is completed.  This will be called even if the cache is already loaded.
        /// </summary>
        public event EventHandler<LoadCompletedEventArgs> LoadCompleted;

        /// <summary>
        /// Clears all data from the disk and memory.
        /// </summary>
        public void ClearCache()
        {
            ThrowIfDisposed();

            // Lock this one to ensure that it blocks if the cache is being loaded in another thread.  Also
            // prevents the cache from being loaded while the clear is being performed.
            lock (_loadLock)
            {
                // Lock here to ensure that this method doesn't overload with other method calls, including
                // SaveChanges and synchronization.
                using (_saveSyncLock.LockObject())
                {
                    // If loaded, clear the in-memory data.
                    if (_loaded)
                    {
                        _cacheData.Clear();
                    }

                    // Delete storage internal changes cache and the files.
                    _storageHandler.ClearCache();
                }
            }
        }

        /// <summary>
        /// A preinitialized CacheController which can be used to synchronize the context with the uri specified
        /// in the constructor.
        /// </summary>
        public CacheController CacheController 
        {
            get
            {
                ThrowIfDisposed();
                return _cacheController;
            }
        }

        #region OfflineSyncProvider

        /// <summary>
        /// OfflineSyncProvider method called when the controller is about to start a sync session.
        /// </summary>
        public override void BeginSession()
        {
            ThrowIfDisposed();

            // Don't start a second session if sync is already active.
            if (_syncActive)
            {
                throw new InvalidOperationException("Sync session already active for context");
            }

            //Reset IsFirst Sync. This will be set only when the server blob is null;
            _isFirstSync = false;

            // Load the cache if it is not already loaded.
            Load();

            // Lock everything else out while sync is happening.
            _saveSyncLock.Lock();
            _syncActive = true;
        }

        /// <summary>
        /// OfflineSyncProvider method implementation to return a set of sync changes.
        /// </summary>
        /// <param name="state">A unique identifier for the changes that are uploaded</param>
        /// <returns>The set of incremental changes to send to the service</returns>
        public override ChangeSet GetChangeSet(Guid state)
        {
            ThrowIfDisposed();

            if (!_syncActive)
            {
                throw new InvalidOperationException("GetChangeSet cannot be called without calling BeginSession");
            }

            ChangeSet changeSet = new ChangeSet();

            // Get the changes from the storage layer (not the in-memory data that can change)
            IEnumerable<IsolatedStorageOfflineEntity> changes = _storageHandler.GetChanges(state);

            // Fill the change list.
            changeSet.Data = (from change in changes select (IOfflineEntity)change).ToList();
            changeSet.IsLastBatch = true;
            changeSet.ServerBlob = _cacheData.AnchorBlob;

            return changeSet;
        }

        /// <summary>
        /// OfflineSyncProvider method implementation called when a change set returned from GetChangeSet has been
        /// successfully uploaded.
        /// </summary>
        /// <param name="state">The unique identifier passed in to the GetChangeSet call.</param>
        /// <param name="response">ChangeSetResponse that contains an updated server blob and any conflicts or errors that
        /// happened on the service.</param>
        public override void OnChangeSetUploaded(Guid state, ChangeSetResponse response)
        {
            ThrowIfDisposed();

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (!_syncActive)
            {
                throw new InvalidOperationException("OnChangeSetUploaded cannot be called without calling BeginSession");
            }

            if (response.Error == null)
            {
                //Sergei Polevikov
                //IEnumerable<IsolatedStorageOfflineEntity> updatedItems = response.UpdatedItems.Cast<IsolatedStorageOfflineEntity>();
                List<IsolatedStorageOfflineEntity> updatedItems = new List<IsolatedStorageOfflineEntity>();
                foreach (IsolatedStorageOfflineEntity updatedItem in response.UpdatedItems)
                {
                    updatedItems.Add(updatedItem);
                }
                //end

                // Notify the disk management that changes uploaded successfully.
                IEnumerable<Conflict> conflicts = _storageHandler.UploadSucceeded(state, response.ServerBlob, response.Conflicts, updatedItems);

                // Update the in-memory representation.
                _cacheData.AddUploadChanges(response.ServerBlob, conflicts, updatedItems, this);
            }
            else
            {
                _storageHandler.UploadFailed(state);
            }
        }

        /// <summary>
        /// Returns the last server blob that the context received during sync
        /// </summary>
        /// <returns>The server blob.  This will be null if the context has not synchronized with the service</returns>
        public override byte[] GetServerBlob()
        {
            ThrowIfDisposed();

            if (!_syncActive)
            {
                throw new InvalidOperationException("GetServerBlob cannot be called without calling BeginSession");
            }

            byte[] serverBlob = _cacheData.AnchorBlob;
            
            if (serverBlob == null)
                _isFirstSync = true;

            return serverBlob;
        }

        /// <summary>
        /// OfflineSyncProvider method called to save changes retrieved from the sync service.
        /// </summary>
        /// <param name="changeSet">The set of changes from the service to save. Also contains an updated server
        /// blob.</param>
        public override void SaveChangeSet(ChangeSet changeSet)
        {
            ThrowIfDisposed();

            if (changeSet == null)
            {
                throw new ArgumentNullException("changeSet");
            }

            if (!_syncActive)
            {
                throw new InvalidOperationException("SaveChangeSet cannot be called without calling BeginSession");
            }

            if (changeSet.Data.Count == 0 && !_isFirstSync)
            {
                return;
            }
            
            //Sergei Polevikov
            // Cast to the isolated storage-specific entity.
            //IEnumerable<IsolatedStorageOfflineEntity> entities = changeSet.Data.Cast<IsolatedStorageOfflineEntity>();
            List<IsolatedStorageOfflineEntity> entities = new List<IsolatedStorageOfflineEntity>();
            foreach (IsolatedStorageOfflineEntity entity in changeSet.Data)
            {
                entities.Add(entity);
            }
            //end
            
            // Store the downloaded changes to disk.
            _storageHandler.SaveDownloadedChanges(changeSet.ServerBlob, entities, _isFirstSync);

            // Update in-memory representation.
            _cacheData.DownloadedChanges(changeSet.ServerBlob, entities);

            GC.Collect();
        }

        /// <summary>
        /// OfflineSyncProvider method called when sync is completed.  This method will unlock so that SaveChanges
        /// and other operations can be called.
        /// </summary>
        public override void EndSession()
        {
            ThrowIfDisposed();

            // If sync is not active, throw.  The context doesn't need to worry about exiting the lock if this throws
            // because it can only be set to false outside of the lock.
            if (!_syncActive)
            {
                throw new InvalidOperationException("Sync session not active for context");
            }

            //If this is first sync then call notfication as a reset instead of Add for every item.
            if (_isFirstSync)
            {
                _cacheData.NotifyAllCollections();
            }

            // Sync is no longer active
            _syncActive = false;

            // Unlock.
            _saveSyncLock.Unlock();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose the context and releases the lock on the cache path
        /// </summary>
        public void Dispose()
        {
            // This is the standard dispose pattern
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose internal references
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_storageHandler != null)
                    {
                        _storageHandler.Dispose();
                        _storageHandler = null;
                    }

                    if (_saveSyncLock != null)
                    {
                        _saveSyncLock.Dispose();
                        _saveSyncLock = null;
                    }
                }

                _isDisposed = true;
            }
        }


        /// <summary>
        /// Method which checks whether or not the object is disposed and throws if it is
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot access a disposed IsolatedStorageOfflineContext");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the cache controller to sync with the context.
        /// </summary>
        private void CreateCacheController()
        {
            _cacheController = new CacheController(_scopeUri, _scopeName, this);

            CacheControllerBehavior behavior = _cacheController.ControllerBehavior;
            foreach (EntityType t in _schema.Collections)
            {
                behavior.AddType(t);
            }
        }

        /// <summary>
        /// Thread method used when LoadAsync is called.
        /// </summary>
        /// <param name="state">Required by ThreadPool, but not used</param>
        private void LoadThread(object state)
        {
            Exception exception = null;

            try
            {
                // Double-checking lock to ensure that loading is actually necesary.
                if (!_loaded)
                {
                    lock (_loadLock)
                    {
                        if (!_loaded)
                        {
                            LoadInternal();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }

                // Catch the exception and store it.
                exception = e;
            }

            // Pass the event args (including the exception to the callback).
            EventHandler<LoadCompletedEventArgs> loadCompleted = LoadCompleted;
            if (loadCompleted != null)
            {
                loadCompleted(this, new LoadCompletedEventArgs(exception));
            }
        }

        /// <summary>
        /// Loads the data from the cache without doing any locking.
        /// </summary>
        private void LoadInternal()
        {
            // Verify the schema and uri match was was previously used for the cache path.
            //CheckSchemaAndUri(_cachePath, _schema, _scopeUri, _scopeName, StorageHandler.EncryptionAlgorithm);

            // Load the data.
            _cacheData = _storageHandler.Load(this);

            _loaded = true;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// This member stores the in-memory data for the context.  It is returned by the
        /// _storageHandler.Load method when Load is executed
        /// </summary>
        private SQLiteCacheData _cacheData = null;

        /// <summary>
        /// This member manages the disk access for the context. It is instantiated in the
        /// constructor.
        /// </summary>
        private SQLiteStorageHandler _storageHandler = null;

        /// <summary>
        /// Specifies whether or not the context is loaded.  It is set when the context has been
        /// successfully loaded. It is guared by the _loadLock
        /// </summary>
        private volatile bool _loaded = false;

        // Note: When these locks are nested, _loadLock should always be used before _saveSyncLock
        // to avoid deadlocks.

        /// <summary>
        /// This lock guards the _loaded variable.  It is used to cause multiple threads attempting
        /// to load to block.
        /// </summary>
        private object _loadLock = new object();

        /// <summary>
        /// Essentially guards the _cacheData object.  Prevents multiple accesses that result in
        /// modification of the _cacheData object.  Also used to prevent save during sync.
        /// </summary>
        private AutoResetLock _saveSyncLock;

        /// <summary>
        /// The scope uri for the context.  Passed in to the constructor.
        /// </summary>
        private Uri _scopeUri;

        /// <summary>
        /// The scope name for the context. Passed in to the constructor.
        /// </summary>
        private string _scopeName;

        /// <summary>
        /// The cache path for the context. Passed in to the constructor.
        /// </summary>
        private string _cachePath;

        /// <summary>
        /// Cache controller generated as a convenience for the user.  Created
        /// in the constructor.
        /// </summary>
        private CacheController _cacheController;

        /// <summary>
        /// Schema passed in to the constructor.  Passed to the storage handler
        /// so that the appropriate collections can be created.
        /// </summary>
        private IsolatedStorageSchema _schema;

        /// <summary>
        /// Specifies that sync is active.  It is set to true in BeginSession and 
        /// set to false in EndSession.
        /// </summary>
        private volatile bool _syncActive = false;

        /// <summary>
        /// Specifies that the context has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Used to detect if this is the first sync to the server.
        /// This is used to send notification to the databinder only once
        /// instead of each item.
        /// </summary>
        bool _isFirstSync = false;
        
        #endregion
    }

}
