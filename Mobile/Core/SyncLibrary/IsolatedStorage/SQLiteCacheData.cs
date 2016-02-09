using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Common.Entites;
using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;

namespace BitMobile.SyncLibrary.IsolatedStorage
{
    internal class SQLiteCacheData
    {
//        public Dictionary<EntityType, IsolatedStorageCollection> Collections { get; set; }
        public List<SyncConflict> SyncConflicts { get; set; }
        public List<SyncError> SyncErrors { get; set; }
        public byte[] AnchorBlob 
        {
            get
            {
                if (DbEngine.Database.Current.IsSynced())
                    return DbEngine.Database.Current.LoadAnchor();
                else
                    return null;
            }
            set
            {
                if (value != null)
                    DbEngine.Database.Current.SaveAnchor(value);
            }
        }

        public SQLiteCacheData(IsolatedStorageSchema schema, IsolatedStorageOfflineContext context)
        {
//            Collections = new Dictionary<EntityType, IsolatedStorageCollection>();
            SyncConflicts = new List<SyncConflict>();
            SyncErrors = new List<SyncError>();

            CreateCollections(schema, context);
        }

        public void DownloadedChanges(byte[] anchor, IEnumerable<IsolatedStorageOfflineEntity> entities)
        {
            AnchorBlob = anchor;
        }

        public void AddUploadChanges(byte[] anchor, IEnumerable<Conflict> conflicts, IEnumerable<IsolatedStorageOfflineEntity> updatedItems, IsolatedStorageOfflineContext context)
        {
        }

        public void AddSerializedDownloadResponse(byte[] anchor, IEnumerable<IsolatedStorageOfflineEntity> entities)
        {
        }

        public void AddSerializedUploadResponse(byte[] anchor, IEnumerable<IsolatedStorageOfflineEntity> entities)
        {
        }

        public void AddSerializedLocalChanges(IEnumerable<IsolatedStorageOfflineEntity> entities)
        {
        }

        public void AddSerializedLocalChange(IsolatedStorageOfflineEntity entity)
        {
        }

        public void AddSerializedDownloadItem(IsolatedStorageOfflineEntity entity)
        {
        }

        public IEnumerable<IsolatedStorageOfflineEntity> CommitChanges()
        {
            List<IsolatedStorageOfflineEntity> changes = new List<IsolatedStorageOfflineEntity>();

            // Loop over each collection and have it commit the changes in it
//            foreach (var collection in Collections.Values)
//            {
//                changes.AddRange(collection.CommitChanges());
//            }

            return changes;
        }

        public ICollection<IsolatedStorageOfflineEntity> GetSaveFailures()
        {
            return new List<IsolatedStorageOfflineEntity>();
        }

        public void Rollback()
        {
        }

        public void ResolveStoreConflictByRollback(IsolatedStorageOfflineEntity entity)
        {
        }

        public void AddConflicts(IEnumerable<SyncConflict> conflicts, IsolatedStorageOfflineContext context)
        {
        }

        public void AddSerializedConflict(SyncConflict conflict, IsolatedStorageOfflineContext context)
        {
        }

        public void AddErrors(IEnumerable<SyncError> errors, IsolatedStorageOfflineContext context)
        {
        }

        public void AddSyncError(SyncError error, IsolatedStorageOfflineContext context)
        {
        }

        public void AddSerializedError(SyncError error, IsolatedStorageOfflineContext context)
        {
        }

        public void RemoveSyncConflict(SyncConflict conflict)
        {
        }

        public void RemoveSyncError(SyncError error)
        { 
        }

        public void Clear()
        {
            ClearCollections();
            SyncConflicts.Clear();
            SyncErrors.Clear();
            AnchorBlob = null;
        }

        public void ClearCollections()
        {
//            foreach (IsolatedStorageCollection collection in Collections.Values)
//            {
//                collection.Clear();
//            }

            AnchorBlob = null;
        }

        private void ClearSyncConflict(SyncConflict syncConflict, IsolatedStorageOfflineContext context)
        {
        }

        public void ClearSyncConflicts()
        {
        }

        public void ClearSyncErrors()
        {
        }

        internal void NotifyAllCollections()
        {
        }

        private void CreateCollections(IsolatedStorageSchema schema, IsolatedStorageOfflineContext context)
        {
//            Type collectionType = typeof(IsolatedStorageCollection<>);
//            foreach (EntityType t in schema.Collections)
//            {
//                // CreateInstance the generic type for the type in the collection.
//                Type generic = collectionType.MakeGenericType(t);
//                IsolatedStorageCollection collection = (IsolatedStorageCollection)Activator.CreateInstance(generic, context);
//                Collections[t] = collection;
//            }
        }
    }
}
