using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database
    {
        public bool InTransaction()
        {
            return Exec(string.Format("SELECT DISTINCT [TableName] FROM {0}", TranStatusTable), cmd =>
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                    return r.HasRows;
            });
        }

        public void CommitTransaction()
        {
            var lst = new List<string>();
            Exec(string.Format("SELECT DISTINCT [TableName] FROM {0}", TranStatusTable), cmd =>
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                        lst.Add(r.GetString(0));
            });

            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    foreach (string tableName in lst)
                    {
                        Exec(string.Format("DELETE FROM __{0}", tableName), cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        });
                    }
                    Exec(string.Format("DELETE FROM {0}", TranStatusTable), cmd =>
                    {
                        cmd.Transaction = tran;
                        cmd.ExecuteNonQuery();
                    });

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public void RollbackTransaction()
        {
            var toRemoveFromCache = new List<Guid>();
            var modified = new Dictionary<string, List<string>>();
            var inserted = new Dictionary<string, List<string>>();
                         
            Exec(string.Format("SELECT [Id], [TableName], [Status] FROM {0} ORDER BY [TableName]", TranStatusTable), cmd =>
            {
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String id = r.GetString(0);
                        String tableName = r.GetString(1);
                        Operation status = (Operation)r.GetInt16(2);
                        Dictionary<String, List<String>> dict = status != Operation.Insert ? modified : inserted;

                        List<String> ids;
                        if (!dict.TryGetValue(tableName, out ids))
                        {
                            ids = new List<string>();
                            dict.Add(tableName, ids);
                        }
                        ids.Add(id);
                    }
                }
            });

            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    foreach (KeyValuePair<String, List<String>> pair in modified)
                    {
                        String ids = "";
                        foreach (String s in pair.Value)
                        {
                            ids = ids + (String.IsNullOrEmpty(ids) ? String.Format("'{0}'", s) : String.Format(",'{0}'", s));
                            toRemoveFromCache.Add(DbRef.FromString(s).Id);
                        }
                        Exec(string.Format("INSERT OR REPLACE INTO _{0} SELECT * FROM __{0} WHERE [Id] IN ({1})", pair.Key, ids)
                            , cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        });
                        Exec(string.Format("DELETE FROM __{0}", pair.Key), cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        });
                    }
                    foreach (KeyValuePair<String, List<String>> pair in inserted)
                    {
                        string ids = "";
                        foreach (string s in pair.Value)
                        {
                            ids = ids + (String.IsNullOrEmpty(ids) ? String.Format("'{0}'", s) : String.Format(",'{0}'", s));
                            toRemoveFromCache.Add(DbRef.FromString(s).Id);
                        }
                        Exec(string.Format("DELETE FROM _{0} WHERE [Id] IN ({1})", pair.Key, ids), cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        });
                    }
                    Exec(string.Format("DELETE FROM {0}", TranStatusTable), cmd =>
                    {
                        cmd.Transaction = tran;
                        cmd.ExecuteNonQuery();
                    });

                    tran.Commit();

                    _cache.Clear(toRemoveFromCache);
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        private void CopyTranObject(SqliteTransaction tran, String tableName, String id, Operation status)
        {
            // !!! dont use Exec because this function is used in lock(_dbsync)
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
