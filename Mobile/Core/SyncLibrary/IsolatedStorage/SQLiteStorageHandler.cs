using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using BitMobile.Common.Entites;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using Microsoft.Synchronization.ClientServices;

using BitMobile.DbEngine;

namespace BitMobile.SyncLibrary.IsolatedStorage
{
    class SQLiteStorageHandler : IDisposable
    {
        private readonly IsolatedStorageSchema _schema;
        private string _cachePath;
        private readonly List<EntityType> _knownTypes;
        private readonly SymmetricAlgorithm _encryptionAlgorithm;
        private byte[] _anchor;
        private readonly IsolatedStorageOfflineContext _context;

        public SQLiteStorageHandler(IsolatedStorageOfflineContext ctx, IsolatedStorageSchema schema, string cachePath, SymmetricAlgorithm encryptionAlgorithm)
        {
            _context = ctx;
            _schema = schema;
            _cachePath = cachePath;
            _encryptionAlgorithm = encryptionAlgorithm;
            _anchor = null;

            _knownTypes = new List<EntityType>
            {
//                new EntityType(typeof (SyncConflict)),
//                new EntityType(typeof (SyncError))
            };

            AddKnownTypes();
        }

        public SQLiteCacheData Load(IsolatedStorageOfflineContext context)
        {
            return new SQLiteCacheData(_schema, context);
        }

        public void SaveChanges(IEnumerable<IsolatedStorageOfflineEntity> changes)
        {
            using (var db = Database.Current)
            {
                foreach (EntityType t in GetKnownTypes())
                {
                    db.ProcessData(changes, ProcessMode.LocalChanges);
                }
            }
        }

        public IEnumerable<IsolatedStorageOfflineEntity> GetChanges(Guid state)
        {
            var list = new List<IsolatedStorageOfflineEntity>();
            using (var db = Database.Current)
            {
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

        public void SaveDownloadedChanges(byte[] anchor, IEnumerable<IsolatedStorageOfflineEntity> entities, bool isFirstSync)
        {
            using (var db = Database.Current)
            {
                // db.Open();
                if (isFirstSync)
                {
                    db.CreateEntityTables(_knownTypes.ToArray());
                    db.ProcessData(entities, ProcessMode.InitialLoad);
                    db.InitialLoadComplete(_context.CacheController.ControllerBehavior.UserId
                        , _context.CacheController.ControllerBehavior.UserEmail);
                }
                else
                    db.ProcessData(entities, ProcessMode.ServerChanges);
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
