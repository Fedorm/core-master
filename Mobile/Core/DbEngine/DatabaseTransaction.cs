using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database
    {
        public bool InTransaction()
        {
            using (var cmd = new SqliteCommand(String.Format("SELECT DISTINCT [TableName] FROM {0}", TranStatusTable), ActiveConnection))
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    return r.HasRows;
                }
            }
        }

        public void CommitTransaction()
        {
            var lst = new List<string>();
            using (var cmd = new SqliteCommand(String.Format("SELECT DISTINCT [TableName] FROM {0}", TranStatusTable), ActiveConnection))
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lst.Add(r.GetString(0));
                    }
                }
            }

            SqliteTransaction tran = ActiveConnection.BeginTransaction();
            try
            {
                foreach (String tableName in lst)
                {
                    using (var cmd = new SqliteCommand(String.Format("DELETE FROM __{0}", tableName), tran.Connection, tran))
                        cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqliteCommand(String.Format("DELETE FROM {0}", TranStatusTable), tran.Connection, tran))
                    cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void RollbackTransaction()
        {
            var toRemoveFromCache = new List<Guid>();
            var lst1 = new Dictionary<string, List<string>>();
            var lst2 = new Dictionary<string, List<string>>();

            using (var cmd = new SqliteCommand(String.Format("SELECT [Id], [TableName], [Status] FROM {0} ORDER BY [TableName]", TranStatusTable), ActiveConnection))
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String id = r.GetString(0);
                        String tableName = r.GetString(1);
                        int status = r.GetInt16(2);
                        Dictionary<String, List<String>> dict = status != 0 ? lst1 : lst2;

                        List<String> ids;
                        if (!dict.TryGetValue(tableName, out ids))
                        {
                            ids = new List<string>();
                            dict.Add(tableName, ids);
                        }
                        ids.Add(id);
                    }
                }
            }

            SqliteTransaction tran = ActiveConnection.BeginTransaction();
            try
            {
                foreach (KeyValuePair<String, List<String>> pair in lst1)
                {
                    String ids = "";
                    foreach (String s in pair.Value)
                    {
                        ids = ids + (String.IsNullOrEmpty(ids) ? String.Format("'{0}'", s) : String.Format(",'{0}'", s));
                        toRemoveFromCache.Add(DbRef.FromString(s).Id);
                    }
                    using (var cmd = new SqliteCommand(String.Format("INSERT OR REPLACE INTO _{0} SELECT * FROM __{0} WHERE [Id] IN ({1})", pair.Key, ids), tran.Connection, tran))
                        cmd.ExecuteNonQuery();
                    using (var cmd = new SqliteCommand(String.Format("DELETE FROM __{0}", pair.Key), tran.Connection, tran))
                        cmd.ExecuteNonQuery();
                }
                foreach (KeyValuePair<String, List<String>> pair in lst2)
                {
                    String ids = "";
                    foreach (String s in pair.Value)
                    {
                        ids = ids + (String.IsNullOrEmpty(ids) ? String.Format("'{0}'", s) : String.Format(",'{0}'", s));
                        toRemoveFromCache.Add(DbRef.FromString(s).Id);
                    }
                    using (var cmd = new SqliteCommand(String.Format("DELETE FROM _{0} WHERE [Id] IN ({1})", pair.Key, ids), tran.Connection, tran))
                        cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqliteCommand(String.Format("DELETE FROM {0}", TranStatusTable), tran.Connection, tran))
                    cmd.ExecuteNonQuery();

                tran.Commit();

                _cache.Clear(toRemoveFromCache);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        private void CopyTranObject(object obj, SqliteTransaction tran, String tableName, String id, int status)
        {
            using (var cmd = new SqliteCommand(String.Format("INSERT OR IGNORE INTO {0}([Id],[TableName],[Status]) VALUES(@Id,@TableName,@Status)", TranStatusTable), tran.Connection, tran))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.ExecuteNonQuery();
            }

            if (status != 0)
            {
                using (var cmd = new SqliteCommand(String.Format("INSERT OR IGNORE INTO __{0} SELECT * FROM _{0} WHERE [Id] = @Id", tableName), tran.Connection, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
