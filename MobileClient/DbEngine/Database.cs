using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using BitMobile.Application.Entites;
using BitMobile.Application.IO;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.ValueStack;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database : IDisposable, IDatabase
    {
        private static String _fileName = "data.db";

        private DbCache _cache;
        private Dictionary<String, SqliteCommand> _commands;
        private Dictionary<IEntityType, string[]> _columnsByType;
        private SqliteConnection _connection;
        private object _dbsync = new object();
        private static Database _db;

        internal DbCache Cache
        {
            get
            {
                return _cache;
            }
        }

        public static void Init(String solutionUri)
        {
            var uri = new UriBuilder(solutionUri).Uri;
            string solutionName = uri.Segments[uri.Segments.Length - 1];
            solutionName = solutionName.Replace("/", "");
            solutionName = solutionName.Replace("\\", "");
            _fileName = String.Format("{0}_{1}.db", uri.Host, solutionName);
            _db = CreateInstance();
        }

        public static Database Current
        {
            get
            {
                return _db;
            }
        }

        public static String GetDbPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _fileName);
        }

        private static Database CreateInstance()
        {
            if (_db == null)
            {
                _db = new Database
                {
                    _connection = new SqliteConnection(String.Format("Data source={0};journal mode=WAL", GetDbPath())),
                    _commands = new Dictionary<string, SqliteCommand>(),
                    _columnsByType = new Dictionary<IEntityType, string[]>()
                };
                _db._cache = new DbCache(_db);

                AddSupportedTypes();
            }

            return _db;
        }

        public void OnStart()
        {
            if (_db.IsSynced())
                RollbackTransaction();
        }

        private SqliteConnection ActiveConnection
        {
            get
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    InitFunctions();
                }
                return _connection;
            }
        }

        public byte[] GetBackup()
        {
            lock (_dbsync)
            {
                ExecuteNonQuery("pragma wal_checkpoint;");
                return File.ReadAllBytes(GetDbPath());
            }
        }


        public void RestoreBackup(byte[] db)
        {
            lock (_dbsync)
            {
                ExecuteNonQuery("pragma wal_checkpoint;");
                DeleteDatabase();

                File.WriteAllBytes(GetDbPath(), db);
                _connection.Open();
                InitFunctions();
                _db._cache = new DbCache(_db);
            }
        }

        private void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }


        bool? _isSynced;
        String _userId;
        DateTime _lastSync;

        public string UserId
        {
            get
            {
                return _userId;
            }
        }

        public DateTime LastSyncTime
        {
            get
            {
                return _lastSync;
            }
        }

        public string UserEmail { get; private set; }

        public string ResourceVersion { get; private set; }

        public bool SuccessSync { get; private set; }

        public bool IsSynced()
        {
            if (!DbExists())
                return false;
            if (_isSynced != null)
                return (bool)_isSynced;
            try
            {
                var select = string.Format("SELECT [UserId], [LastSync], [LastSyncError] FROM {0} LIMIT 1", DbStatusTable);
                Exec(select, cmd =>
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            _userId = r.GetString(0);
                            DateTime lastSuccessSync = r.GetValue(1) is DBNull ? DateTime.MinValue : r.GetDateTime(1);
                            DateTime lastErrorSync = r.GetValue(2) is DBNull ? DateTime.MinValue : r.GetDateTime(2);

                            SuccessSync = lastSuccessSync > lastErrorSync;
                            _lastSync = SuccessSync ? lastSuccessSync : lastErrorSync;
                        }
                        else
                            _userId = null;
                    }
                });
                _isSynced = _userId != null;

                if (_isSynced.Value)
                {
                    string emails = string.Format("SELECT [Email] FROM {0} WHERE [UserId] = @userId", DbUserInfo);
                    Exec(emails, cmd =>
                    {
                        cmd.Parameters.AddWithValue("userId", _userId);
                        object email = cmd.ExecuteScalar();
                        UserEmail = email is DBNull ? null : email as string;
                    });
                    string values = string.Format("SELECT [Value] FROM {0} WHERE [Key] = @key", SolutionInfoTable);
                    Exec(values, cmd =>
                    {
                        cmd.Parameters.AddWithValue("key", "version");
                        object version = cmd.ExecuteScalar();
                        ResourceVersion = version is DBNull ? null : version as string;
                    });
                }
            }
            catch
            {
                _isSynced = false;
            }
            return (bool)_isSynced;
        }

        public IDataReader Select(String query, params object[] arguments)
        {
            return Exec(query, cmd =>
            {
                cmd.Connection = ActiveConnection;
                cmd.CommandText = query;

                var n = 1;
                foreach (object p in arguments)
                {
                    object value = p;
                    if (p is DateTime)// Bug of DateTime convert in Sqlite: https://bitmobile.atlassian.net/browse/PLA-292
                        value = ((DateTime)p).ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);

                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), value);
                    n++;
                }

                return cmd.ExecuteReader();
            });
        }

        public DataTable SelectAsDataTable(String name, String query, params object[] arguments)
        {
            return Exec(query, cmd =>
            {
                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                    n++;
                }

                var table = new DataTable(name);
                var a = new SqliteDataAdapter(cmd);
                a.Fill(table);

                return table;
            });
        }

        public object SelectScalar(string query, params object[] arguments)
        {
            return Exec(query, cmd =>
            {
                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(string.Format("@p{0}", n), p);
                    n++;
                }

                object result = cmd.ExecuteScalar();
                if (result == DBNull.Value || result == null)
                    return null;
                if (DbRef.CheckIsRef(result.ToString()))
                    return DbRef.FromString(result.ToString());
                return result;
            });
        }

        public int ExecuteNonQuery(string query, params object[] arguments)
        {
            return Exec(query, cmd =>
            {
                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                    n++;
                }

                return cmd.ExecuteNonQuery();
            });
        }

        public void Delete(IDbRef obj, bool inTran)
        {
            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    if (!inTran)
                    {
                        string tableName = obj.TableName;

                        Exec(string.Format("DELETE FROM __{0} WHERE [Id]=@Id", tableName), cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.Parameters.AddWithValue("@Id", obj.ToString());
                            cmd.ExecuteNonQuery();
                        });
                        string q = string.Format("DELETE FROM {0} WHERE [Id]=@Id AND [TableName]=@TableName", TranStatusTable);
                        Exec(q, cmd =>
                        {
                            cmd.Transaction = tran;
                            cmd.Parameters.AddWithValue("@Id", obj.ToString());
                            cmd.Parameters.AddWithValue("@TableName", tableName);
                            cmd.ExecuteNonQuery();
                        });
                    }
                    else
                        lock (_dbsync)
                            CopyTranObject(tran, obj.TableName, obj.ToString(), Operation.Delete);

                    Exec(string.Format("UPDATE [_{0}] SET IsTombstone = 1 WHERE [Id]=@Id", obj.TableName), cmd =>
                    {
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("@Id", obj.ToString());
                        cmd.ExecuteNonQuery();
                    });

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                }
            }
        }

        private IEnumerable<IEntity> Select(IEntityType type, bool useView, String where, params object[] arguments)
        {
            lock (_dbsync)
            {
                string[] columns = GetColumns(type);
                int n;
                String cmdKey = type.Name + where + useView;
                SqliteCommand cmd;
                if (!_commands.TryGetValue(cmdKey, out cmd))
                {
                    String tableName = type.TableName;
                    String columnNames = "";
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (string column in columns)
                        columnNames = columnNames + string.Format("{0}{1}", string.IsNullOrEmpty(columnNames) ? "" : ",", column);
                    if (!useView)
                        columnNames = columnNames + ",IsTombstone";
                    columnNames = columnNames + ",IsDirty";

                    cmd = new SqliteCommand
                    {
                        Connection = ActiveConnection,
                        CommandText =
                            String.Format("SELECT {0} FROM [{1}]{2}", columnNames, useView ? tableName : "_" + tableName,
                                String.IsNullOrEmpty(@where) ? "" : " " + @where)
                    };
                    n = 1;
                    foreach (object p in arguments)
                    {
                        cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                        n++;
                    }
                    cmd.Prepare();
                    _commands.Add(cmdKey, cmd);
                }
                else
                {
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        cmd.Parameters[i].Value = arguments[i];
                    }
                }

                var list = new List<IEntity>();
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    if (r.HasRows)
                    {
                        while (r.Read())
                        {
                            IEntity entity = EntityFactory.CreateInstance(type);
                            n = 0;
                            foreach (string column in columns)
                            {
                                Type t = type.GetPropertyType(column);
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    t = Nullable.GetUnderlyingType(t);

                                object value = r[n];
                                if (value is DBNull)
                                    value = null;
                                else
                                {
                                    if (t == typeof(IDbRef))
                                        value = DbRef.FromString(value.ToString());
                                    else
                                    {
                                        if (t == typeof(Guid))
                                        {
                                            Guid g;
                                            if (!Guid.TryParse(value.ToString(), out g))
                                            {
                                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                                    value = null;
                                                else
                                                    throw new ArgumentException(String.Format("Can't convert value '{0}' to System.Guid", value));
                                            }
                                            else
                                                value = g;
                                        }
                                        else
                                            value = Convert.ChangeType(value, t);
                                    }
                                }
                                entity.SetValue(column, value);
                                n++;
                            }

                            bool isTombstone = !useView && (long)r["IsTombstone"] != 0;

                            var sqliteEntity = (ISqliteEntity)entity;
                            sqliteEntity.IsTombstone = isTombstone;
                            sqliteEntity.Load((long)r["IsDirty"] != 0);

                            list.Add(entity);
                        }
                    }
                }
                return list;
            }
        }

        public IEntity SelectById(IEntityType type, String guid)
        {
            return Select(type, true, "WHERE [Id] = @p1", guid).FirstOrDefault();
        }

        public Stream SelectStream(string query, params object[] arguments)
        {
            using (IDataReader r = Select(query, arguments))
            {
                string data = null;
                if (r.Read())
                    data = r[0].ToString();
                if (data != null)
                    return new MemoryStream(Convert.FromBase64String(data));
            }
            return null;
        }

        public IEnumerable SelectDirty(IEntityType type)
        {
            return Select(type, false, "WHERE [IsDirty] = 1 OR [IsTombstone] = 1");
        }

        public void InitialLoadComplete(string userId, string userEmail, string resourceVersion)
        {
            UpdateDbStatus(userId, true);
            UpdateUserInfo(userId, userEmail);
            AddSolutionInfo("version", resourceVersion);
            CloseConnection();
        }

        public void UpdateConnectionInfo(string userId, string userEmail, string resourceVersion)
        {
            UpdateUserInfo(userId, userEmail);
            AddSolutionInfo("version", resourceVersion);
        }

        public void Save(IEntity obj, bool inTran)
        {
            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    ProcessAllInternal(new[] { obj }, ProcessMode.LocalChanges, tran, inTran);
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                }
            }
        }

        public void SelectInto(string tableName, string query, params object[] arguments)
        {
            query = string.Format("INSERT INTO [{0}] {1}", GetUserTable(tableName), query);
            Exec(query, cmd =>
            {
                cmd.Connection = ActiveConnection;
                cmd.CommandText = query;

                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                    n++;
                }

                cmd.ExecuteNonQuery();
            });
        }

        public void CreateUserTable(string tableName, params string[] columns)
        {
            string s = "";
            foreach (string c in columns)
            {
                if (!string.IsNullOrEmpty(s))
                    s = s + ",";
                s = s + string.Format("[{0}] TEXT", c);
            }

            Exec(string.Format("CREATE TABLE [{0}]({1})", GetUserTable(tableName), s)
                , cmd => cmd.ExecuteNonQuery());
        }

        public void DropUserTable(string tableName)
        {
            Exec(string.Format("DROP TABLE [{0}]", GetUserTable(tableName))
                , cmd => cmd.ExecuteNonQuery());
        }

        public void TruncateUserTable(string tableName)
        {
            Exec(string.Format("DELETE FROM {0}", GetUserTable(tableName))
                , cmd => cmd.ExecuteNonQuery());
        }

        public void ProcessData(IEnumerable<IEntity> data, ProcessMode mode)
        {
            SqliteTransaction tran = null;
            var toRemoveFromCache = new List<Guid>();

            try
            {
                foreach (IEnumerable<IEntity> lst in GetBlock(data.GetEnumerator()))
                {
                    tran = ActiveConnection.BeginTransaction();
                    ProcessAllInternal(lst, mode, tran, mode == ProcessMode.LocalChanges);
                    tran.Commit();
                    tran.Dispose();
                    tran = null;

                    if (mode == ProcessMode.ServerChanges)
                    {
                        foreach (ISqliteEntity e in lst)
                            toRemoveFromCache.Add(e.EntityId);
                    }

                    GC.Collect();
                }
                Cache.Clear(toRemoveFromCache);
            }
            catch
            {
                if (tran != null)
                {
                    tran.Rollback();
                    tran.Dispose();
                }
                throw;
            }
        }

        private static string GetUserTable(string tableName)
        {
            return "UT_" + tableName;
        }

        private static IEnumerable<IEnumerable<IEntity>> GetBlock(IEnumerator<IEntity> data)
        {
            var list = new List<IEntity>();
            IEntity lastObject = null;
            while (data.MoveNext())
            {
                if (lastObject == null)
                    lastObject = data.Current;

                if (lastObject.EntityType.Name == data.Current.EntityType.Name)
                    list.Add(data.Current);
                else
                {
                    yield return list;
                    list.Clear();
                    list.Add(data.Current);
                }

                lastObject = data.Current;
            }
            if (list.Count > 0)
                yield return list;
        }

        private void ProcessAllInternal(IEnumerable<IEntity> data, ProcessMode mode, SqliteTransaction tran, bool inTran)
        {
            string[] columns = null;
            var cmd = new SqliteCommand[4];
            String tableName = null;

            lock (_dbsync)
                try
                {
                    foreach (IEntity obj in data)
                    {
                        if (cmd[(int)Operation.Insert] == null)
                        {
                            IEntityType type = obj.EntityType;
                            columns = GetColumns(type);
                            String fNames = "";
                            String fParameters = "";
                            String fFields = "";
                            foreach (string column in columns)
                            {
                                if (!String.IsNullOrEmpty(fNames))
                                {
                                    fNames = fNames + ",";
                                    fParameters = fParameters + ",";
                                    fFields = fFields + ",";
                                }
                                fNames = fNames + column;
                                fParameters = fParameters + "@" + column;
                                fFields = fFields + String.Format("[{0}] = @{0}", column);
                            }
                            fNames = fNames + ",IsTombstone,IsDirty";
                            fParameters = fParameters + ",@IsTombstone,@IsDirty";
                            fFields = fFields + ",[IsTombstone] = @IsTombstone, [IsDirty] = @IsDirty";

                            tableName = type.TableName;

                            cmd[(int)Operation.Insert] = new SqliteCommand(String.Format("INSERT INTO [_{0}]({1}) VALUES({2})", tableName, fNames, fParameters), ActiveConnection, tran);
                            cmd[(int)Operation.Update] = new SqliteCommand(String.Format("UPDATE [_{0}] SET {1} WHERE [Id] = @Id", tableName, fFields), ActiveConnection, tran);
                            foreach (string column in columns)
                            {
                                cmd[0].Parameters.Add("@" + column, _supportedTypes[GetType(type.GetPropertyType(column))].DbType);
                                cmd[1].Parameters.Add("@" + column, _supportedTypes[GetType(type.GetPropertyType(column))].DbType);
                            }
                            cmd[(int)Operation.Insert].Parameters.Add("@IsTombstone", DbType.Boolean);
                            cmd[(int)Operation.Insert].Parameters.Add("@IsDirty", DbType.Boolean);
                            cmd[(int)Operation.Update].Parameters.Add("@IsTombstone", DbType.Boolean);
                            cmd[(int)Operation.Update].Parameters.Add("@IsDirty", DbType.Boolean);

                            cmd[(int)Operation.Select] = new SqliteCommand(String.Format("SELECT Id FROM [_{0}] WHERE [Id] = @Id", tableName), ActiveConnection, tran);
                            cmd[(int)Operation.Select].Parameters.Add("@Id", DbType.String);

                            cmd[(int)Operation.Delete] = new SqliteCommand(String.Format("DELETE FROM [_{0}] WHERE [Id] = @Id", tableName), ActiveConnection, tran);
                            cmd[(int)Operation.Delete].Parameters.Add("@Id", DbType.String);
                        }

                        //row id
                        DbRef id;
                        if (mode == ProcessMode.InitialLoad || mode == ProcessMode.ServerChanges)
                            id = DbRef.CreateInstance(tableName, ((ISqliteEntity)obj).EntityId);
                        else
                            id = (DbRef)obj.GetValue(obj.EntityType.IdFieldName);

                        Operation operation = Operation.Insert; //insert 
                        if (mode != ProcessMode.InitialLoad)
                        {
                            if (((ISqliteEntity)obj).IsTombstone)
                            {
                                operation = mode == ProcessMode.ServerChanges ? Operation.Delete : Operation.Update;
                            }
                            else
                            {
                                cmd[2].Parameters[0].Value = id;
                                if (cmd[2].ExecuteScalar() != null)
                                    operation = Operation.Update; //update
                            }
                        }

                        //assign values
                        if (operation == Operation.Delete) //delete
                        {
                            cmd[(int)operation].Parameters[0].Value = id;
                        }
                        else
                        {
                            int n = 0;
                            foreach (string column in columns)
                            {
                                cmd[(int)operation].Parameters[n].Value = obj.GetValue(column);
                                n++;
                            }
                            cmd[(int)operation].Parameters[n].Value = ((ISqliteEntity)obj).IsTombstone ? 1 : 0; //IsTombstone
                            var entity = (ISqliteEntity)obj;
                            cmd[(int)operation].Parameters[n + 1].Value =
                                mode == ProcessMode.LocalChanges && (entity.IsNew() || entity.IsModified())
                                ? 1 : 0;  //isDirty
                        }

                        if (mode == ProcessMode.LocalChanges && inTran)
                            CopyTranObject(tran, tableName, id.ToString(), operation);

                        cmd[(int)operation].ExecuteNonQuery();
                    }
                }
                finally
                {
                    foreach (var c in cmd)
                    {
                        if (c != null)
                            c.Dispose();
                    }
                }
        }

        private string[] GetColumns(IEntityType type)
        {
            string[] result;
            if (!_columnsByType.TryGetValue(type, out result))
            {
                result = type.GetColumns();
                _columnsByType.Add(type, result);
            }
            return result;
        }

        public void SaveAnchor(byte[] anchor)
        {
            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    Exec(string.Format("DELETE FROM {0}", AnchorTable), cmd =>
                    {
                        cmd.Transaction = tran;
                        cmd.ExecuteNonQuery();

                        if (anchor != null)
                        {
                            string data = Convert.ToBase64String(anchor);
                            cmd.CommandText = String.Format("INSERT INTO {0}([Data]) VALUES(@Data)", AnchorTable);
                            cmd.Parameters.AddWithValue("@Data", data);
                            cmd.ExecuteNonQuery();
                        }
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

        public byte[] LoadAnchor()
        {
            return Exec(string.Format("SELECT Data FROM {0} LIMIT 1", AnchorTable), cmd =>
            {
                object data = cmd.ExecuteScalar();
                if (data == DBNull.Value || data == null)
                    return null;
                return Convert.FromBase64String(data.ToString());
            });
        }

        public void SyncComplete(bool success)
        {
            if (success)
            {
                using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
                {
                    try
                    {
                        CreateSchema();
                        foreach (string tableName in _tablesByIdx)
                        {
                            if (tableName.StartsWith("_Catalog_") || tableName.StartsWith("_Document_"))
                            {
                                Exec(string.Format("UPDATE {0} SET IsDirty = 0 WHERE IsDirty = 1", tableName), cmd =>
                                {
                                    cmd.Transaction = tran;
                                    cmd.ExecuteNonQuery();
                                });
                            }
                        }
                        Cache.ClearNew();
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }

            UpdateDbStatus(_userId, success);
        }

        private Type GetType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type);
            return type;
        }

        public void Dispose()
        {
        }

        void UpdateDbStatus(String userId, bool successSync)
        {
            SuccessSync = successSync;
            string query = string.Format("INSERT OR REPLACE INTO {0}([UserId],[{1}]) VALUES(@UserId,@LastSync)"
                , DbStatusTable, SuccessSync ? "LastSync" : "LastSyncError");
            Exec(query, cmd =>
            {
                DateTime ls = DateTime.Now;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@LastSync", ls);
                cmd.ExecuteNonQuery();

                _userId = userId;
                _lastSync = ls;
                _isSynced = true;
            });
        }

        void UpdateUserInfo(String userId, string email)
        {
            string query = string.Format("INSERT OR REPLACE INTO {0}([UserId],[Email]) VALUES(@UserId,@Email)", DbUserInfo);
            Exec(query, cmd =>
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.ExecuteNonQuery();
            });

            UserEmail = email;
        }


        private void AddSolutionInfo(string key, string resourceVersion)
        {
            string query = string.Format("INSERT OR REPLACE INTO {0}([Key],[Value]) VALUES(@p1,@p2)", SolutionInfoTable);
            ExecuteNonQuery(query, key, resourceVersion);

            ResourceVersion = resourceVersion;
        }

        private void Exec(string query, Action<SqliteCommand> action)
        {
            lock (_dbsync)
                using (var cmd = new SqliteCommand(query, ActiveConnection))
                    action(cmd);
        }

        private T Exec<T>(string query, Func<SqliteCommand, T> func)
        {
            lock (_dbsync)
                using (var cmd = new SqliteCommand(query, ActiveConnection))
                    return func(cmd);
        }


        enum Operation
        {
            Insert = 0,
            Update = 1,
            Select = 2,
            Delete = 3
        }
    }

    public class DbColumn
    {
        public String TypeName { get; set; }
        public DbType DbType { get; set; }
    }
}
