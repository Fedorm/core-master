using System;
using System.Collections.Generic;
using BitMobile.Common.Entites;
using BitMobile.ValueStack;

namespace BitMobile.DbEngine
{
    internal class DbCache : IDbCache
    {
        private readonly Database _db;
        private readonly Dictionary<String, EntityType> _types;
        private readonly Dictionary<Guid, DbRef> _refs;
        private readonly Dictionary<String, DbRef> _emptyRefs;

        public DbCache(Database db)
        {
            _db = db;
            _types = new Dictionary<string, EntityType>();
            _refs = new Dictionary<Guid, DbRef>();
            _emptyRefs = new Dictionary<String, DbRef>();
        }

        public IEntity GetObject(DbRef r)
        {
            EntityType t;
            if (!_types.TryGetValue(r.TableName, out t))
            {
                t = EntityFactory.FindType(r.TableName);
                _types.Add(r.TableName, t);
            }
            return _db.SelectById(t, r.ToString());
        }

        public DbRef GetRef(String tableName, Guid id)
        {
            DbRef r;
            if (id.Equals(Guid.Empty))
                r = GetEmptyRef(tableName);
            else
            {
                if (!_refs.TryGetValue(id, out r))
                {
                    r = new DbRef(tableName, id);
                    _refs.Add(id, r);
                }
            }
            return r;
        }

        public DbRef GetEmptyRef(String tableName)
        {
            DbRef r;
            if (!_emptyRefs.TryGetValue(tableName, out r))
            {
                r = new DbRef(tableName, Guid.Empty);
                _emptyRefs.Add(tableName, r);
            }
            return r;
        }

        private String TableNameToTypeName(String tableName)
        {
            int idx = tableName.IndexOf('_');
            return String.Format("DefaultScope.{0}.{1}", tableName.Substring(0, idx), tableName.Substring(idx + 1, tableName.Length - idx - 1));
        }

        public void ClearNew()
        {
            var toDelete = new List<Guid>();
            foreach (KeyValuePair<Guid, DbRef> entry in _refs)
            {
                if (entry.Value.IsNewInternal())
                    toDelete.Add(entry.Key);
            }
            foreach (Guid key in toDelete)
                _refs.Remove(key);
        }

        public void Clear(List<Guid> ids)
        {
            foreach (Guid key in ids)
                _refs.Remove(key);
        }
    }

}
