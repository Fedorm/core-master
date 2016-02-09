using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Application.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.SyncLibrary;
using BitMobile.Common.ValueStack;
using BitMobile.SyncLibrary.BitMobile;
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
                if (DbContext.Current.Database.IsSynced())
                    return DbContext.Current.Database.LoadAnchor();
                else
                    return null;
            }
            set
            {
                if (value != null)
                    DbContext.Current.Database.SaveAnchor(value);
            }
        }

        public SQLiteCacheData(IsolatedStorageSchema schema, IOfflineContext context)
        {
//            Collections = new Dictionary<EntityType, IsolatedStorageCollection>();
            SyncConflicts = new List<SyncConflict>();
            SyncErrors = new List<SyncError>();

            CreateCollections(schema, context);
        }

        public void DownloadedChanges(byte[] anchor)
        {
            AnchorBlob = anchor;
        }

        public void AddUploadChanges(byte[] anchor, IEnumerable<Conflict> conflicts, IEnumerable<IsolatedStorageOfflineEntity> updatedItems, IOfflineContext context)
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

        public void AddConflicts(IEnumerable<SyncConflict> conflicts, OfflineContext context)
        {
        }

        public void AddSerializedConflict(SyncConflict conflict, OfflineContext context)
        {
        }

        public void AddErrors(IEnumerable<SyncError> errors, OfflineContext context)
        {
        }

        public void AddSyncError(SyncError error, OfflineContext context)
        {
        }

        public void AddSerializedError(SyncError error, OfflineContext context)
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

        private void ClearSyncConflict(SyncConflict syncConflict, OfflineContext context)
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

        private void CreateCollections(IsolatedStorageSchema schema, IOfflineContext context)
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
