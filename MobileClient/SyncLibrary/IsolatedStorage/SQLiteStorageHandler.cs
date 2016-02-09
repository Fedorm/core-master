using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Entites;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.SyncLibrary;
using BitMobile.Common.ValueStack;
using BitMobile.SyncLibrary.BitMobile;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using Microsoft.Synchronization.ClientServices;

namespace BitMobile.SyncLibrary.IsolatedStorage
{
    class SQLiteStorageHandler : IDisposable
    {
        private readonly IsolatedStorageSchema _schema;
        private string _cachePath;
        private readonly List<IEntityType> _knownTypes;
        private readonly SymmetricAlgorithm _encryptionAlgorithm;
        private byte[] _anchor;
        private readonly IOfflineContext _context;

        public SQLiteStorageHandler(IOfflineContext ctx, IsolatedStorageSchema schema, string cachePath, SymmetricAlgorithm encryptionAlgorithm)
        {
            _context = ctx;
            _schema = schema;
            _cachePath = cachePath;
            _encryptionAlgorithm = encryptionAlgorithm;
            _anchor = null;

            _knownTypes = new List<IEntityType>();

            AddKnownTypes();
        }

        public SQLiteCacheData Load(IOfflineContext context)
        {
            return new SQLiteCacheData(_schema, context);
        }

        public void SaveChanges(IEnumerable<IsolatedStorageOfflineEntity> changes)
        {
            IDatabase db = DbContext.Current.Database;

            foreach (EntityType t in GetKnownTypes())
            {
                db.ProcessData(changes, ProcessMode.LocalChanges);
            }

        }

        public IEnumerable<IsolatedStorageOfflineEntity> GetChanges(Guid state)
        {
            var list = new List<IsolatedStorageOfflineEntity>();
            var db = DbContext.Current.Database;

            if (db.IsSynced())
            {
                foreach (EntityType t in GetKnownTypes())
                {
                    foreach (IsolatedStorageOfflineEntity entity in db.SelectDirty(t))
                    {
                        list.Add(entity);
                    }
                }
            }

            return list;
        }

        public void UploadFailed(Guid state)
        {
        }

        public IEnumerable<Conflict> UploadSucceeded(Guid state, byte[] anchor, IEnumerable<Conflict> conflicts, IEnumerable<IsolatedStorageOfflineEntity> entities)
        {
            return new List<Conflict>();
        }

        public void SaveDownloadedChanges(IEnumerable<IEntity> entities, bool isFirstSync)
        {
            var db = DbContext.Current.Database;
            {
                // db.Open();
                if (isFirstSync)
                {
                    db.CreateEntityTables(_knownTypes);
                    db.ProcessData(entities, ProcessMode.InitialLoad);
                    db.InitialLoadComplete(_context.CacheController.ControllerBehavior.UserId
                        , _context.CacheController.ControllerBehavior.UserEmail
                        , _context.CacheController.ControllerBehavior.ResourceVersion);
                }
                else
                {
                    db.ProcessData(entities, ProcessMode.ServerChanges);
                    db.UpdateConnectionInfo(_context.CacheController.ControllerBehavior.UserId
                        , _context.CacheController.ControllerBehavior.UserEmail
                        , _context.CacheController.ControllerBehavior.ResourceVersion);
                }
            }
        }

        public void ClearConflicts()
        {
        }

        public void ClearErrors()
        {
            _anchor = null;
        }

        public void ClearCacheFiles()
        {
        }

        public void ClearCache()
        {
            ClearCacheFiles();
        }

        public SymmetricAlgorithm EncryptionAlgorithm
        {
            get
            {
                return _encryptionAlgorithm;
            }
        }

        public void Dispose()
        {

        }

        private void AddKnownTypes()
        {
            foreach (EntityType t in _schema.Collections)
            {
                if (t.IsTable)
                    _knownTypes.Add(t);
            }
        }

        private IEnumerable<EntityType> GetKnownTypes()
        {
            foreach (EntityType t in _knownTypes)
            {
                if (t.IsTable)
                    yield return t;
            }
        }
    }
}
