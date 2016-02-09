using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BitMobile.Common.Entites;
using BitMobile.ValueStack;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database : IDisposable, IDatabase
    {
        private static String _fileName = "data.db";

        private DbCache _cache;
        private Dictionary<String, SqliteCommand> _commands;
        private Dictionary<EntityType, string[]> _columnsByType;
        private SqliteConnection _connection;
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
            var uri = new Uri(solutionUri);
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

        private static String GetDbPath()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _fileName);
        }

        private static Database CreateInstance()
        {
            if (_db == null)
            {
                _db = new Database
                {
                    _connection = new SqliteConnection(String.Format("Data source={0}", GetDbPath())),
                    _commands = new Dictionary<string, SqliteCommand>(),
                    _columnsByType = new Dictionary<EntityType, string[]>()
                };
                _db._cache = new DbCache(_db);

                AddSupportedTypes();
            }

            return _db;
        }

        public static String GetUserTable(String tableName)
        {
            return "UT_" + tableName;
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
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                    InitFunctions();
                }
                return _connection;
            }
        }

        private void CloseConnection()
        {
            if (_connection.State != System.Data.ConnectionState.Closed)
                _connection.Close();
        }


        bool? _isSynced;
        String _userId;
        DateTime _lastSync;

        public String UserId
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

        public bool SuccessSync { get; private set; }

        public bool IsSynced()
        {
            if (!DbExists())
                return false;
            if (_isSynced != null)
                return (bool)_isSynced;
            try
            {
                using (var cmd = new SqliteCommand(String.Format("SELECT [UserId], [LastSync], [LastSyncError] FROM {0} LIMIT 1", DbStatusTable), ActiveConnection))
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
                }
                _isSynced = _userId != null;

                if (_isSynced.Value)
                {
                    using (var cmd = new SqliteCommand(string.Format("SELECT [Email] FROM {0} WHERE [UserId] = @userId", DbUserInfo), ActiveConnection))
                    {
                        cmd.Parameters.AddWithValue("userId", _userId);
                        object email = cmd.ExecuteScalar();
                        UserEmail = email is DBNull ? null : email as string;
                    }
                }
            }
            catch
            {
                _isSynced = false;
            }
            return (bool)_isSynced;
        }

        public SqliteDataReader Select(String query, params object[] arguments)
        {
            using (var cmd = new SqliteCommand())
            {
                cmd.Connection = ActiveConnection;
                cmd.CommandText = query;

                var n = 1;
                foreach (object p in arguments)
                {
                    object value = p;
                    if (p is DateTime)
                        value = ((DateTime)p).ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);

                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), value);
                    n++;
                }

                return cmd.ExecuteReader();
            }
        }

        public void SelectInto(String tableName, String query, params object[] arguments)
        {
            query = String.Format("INSERT INTO [{0}] {1}", GetUserTable(tableName), query);
            using (var cmd = new SqliteCommand())
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
            }
        }

        public System.Data.DataTable SelectAsDataTable(String name, String query, params object[] arguments)
        {
            var cmd = new SqliteCommand { Connection = ActiveConnection, CommandText = query };

            var n = 1;
            foreach (object p in arguments)
            {
                cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                n++;
            }

            var table = new System.Data.DataTable(name);
            var a = new SqliteDataAdapter(cmd);
            a.Fill(table);

            return table;
        }

        public object SelectScalar(String query, params object[] arguments)
        {
            using (var cmd = new SqliteCommand())
            {
                cmd.Connection = ActiveConnection;
                cmd.CommandText = query;

                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                    n++;
                }

                object result = cmd.ExecuteScalar();
                if (result == DBNull.Value || result == null)
                    return null;
                if (DbRef.CheckIsRef(result.ToString()))
                    return DbRef.FromString(result.ToString());
                return result;
            }
        }

        public int ExecuteNonQuery(String query, params object[] arguments)
        {
            using (var cmd = new SqliteCommand())
            {
                cmd.Connection = ActiveConnection;
                cmd.CommandText = query;

                var n = 1;
                foreach (object p in arguments)
                {
                    cmd.Parameters.AddWithValue(String.Format("@p{0}", n), p);
                    n++;
                }

                return cmd.ExecuteNonQuery();
            }
        }

        public void CreateUserTable(String tableName, String[] columns)
        {
            String s = "";
            foreach (String c in columns)
            {
                if(!String.IsNullOrEmpty(s))
                    s = s + ",";
                s = s + String.Format("[{0}] TEXT", c);
            }

            using (var cmd = new SqliteCommand(String.Format("CREATE TABLE [{0}]({1})", GetUserTable(tableName), s), ActiveConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void DropUserTable(String tableName)
        {
            using (var cmd = new SqliteCommand(String.Format("DROP TABLE [{0}]", GetUserTable(tableName)), ActiveConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void TruncateUserTable(String tableName)
        {
            using (var cmd = new SqliteCommand(String.Format("DELETE FROM {0}", GetUserTable(tableName)), ActiveConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(IDbRef obj, bool inTran)
        {
            using (SqliteTransaction tran = ActiveConnection.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqliteCommand(String.Format("UPDATE [_{0}] SET IsTombstone = 1 WHERE [Id]=@Id", obj.TableName), tran.Connection, tran))
                    {
                        cmd.Parameters.AddWithValue("@Id", obj.ToString());
                        cmd.ExecuteNonQuery();
                    }

                    if (!inTran)
                    {
                        String tableName = obj.TableName;

                        using (var cmd = new SqliteCommand(String.Format("DELETE FROM __{0} WHERE [Id]=@Id", tableName), tran.Connection, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", obj.ToString());
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqliteCommand(String.Format("DELETE FROM {0} WHERE [Id]=@Id AND [TableName]=@TableName", TranStatusTable), tran.Connection, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", obj.ToString());
                            cmd.Parameters.AddWithValue("@TableName", tableName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                }
            }
            
        }

        public IEnumerable<IEntity> Select(EntityType type, bool useView, String where, params object[] arguments)
        {
            string[] columns = GetColumns(type);
            int n;
            String cmdKey = type.Name + where + useView;
            SqliteCommand cmd;
            if (!_commands.TryGetValue(cmdKey, out cmd))
            {
                String tableName = type.TableName;
                String columnNames = "";
                foreach (string column in columns)
                    columnNames = columnNames + String.Format("{0}{1}", String.IsNullOrEmpty(columnNames) ? "" : ",", column);
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
                            if (value.GetType().Equals(typeof(DBNull)))
                                value = null;
                            else
                            {
                                if (t.Equals(typeof(IDbRef)))
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
                        (entity as ISqliteEntity).IsTombstone = isTombstone;
                        (entity as ISqliteEntity).Load((long)r["IsDirty"] != 0);

                        list.Add(entity);
                    }
                }
            }
            return list;
        }

        public IEntity SelectById(EntityType type, String guid)
        {
            return Select(type, true, "WHERE [Id] = @p1", guid).FirstOrDefault();
        }

        public System.IO.Stream SelectStream(String query, params object[] arguments)
        {
            using (SqliteDataReader r = Select(query, arguments))
            {
                String data = null;
                int cnt = 0;
                while (r.Read())
                {
                    if (cnt == 0)
                    {
                        data = r[0].ToString();
                    }
                    cnt++;
                }
                if (data != null)
                    return new System.IO.MemoryStream(Convert.FromBase64String(data));
            }
            return null;
        }

        public System.IO.Stream SelectStream_(String query, params object[] arguments)
        {
            using (SqliteDataReader r = Select(query, arguments))
            {
                if (r.Read())
                {
                    String data = r[0].ToString();
                    return new System.IO.MemoryStream(Convert.FromBase64String(data));
                }
            }
            return null;
        }

        public System.Collections.IEnumerable SelectDirty(EntityType type)
        {
            return Select(type, false, "WHERE [IsDirty] = 1 OR [IsTombstone] = 1");
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

        public void ProcessData(IEnumerable<IEntity> data, ProcessMode mode, SqliteTransaction tran = null)
        {
            bool inTran = tran != null;
            var toRemoveFromCache = new List<Guid>();

            try
            {
                foreach (IEnumerable<IEntity> lst in GetBlock(data.GetEnumerator()))
                {
                    if (!inTran)
                        tran = ActiveConnection.BeginTransaction();
                    ProcessAllInternal(lst, mode, tran, mode == ProcessMode.LocalChanges);
                    if (!inTran)
                        tran.Commit();

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
                if (tran != null && !inTran)
                    tran.Rollback();
                throw;
            }
        }

        private static IEnumerable<IEnumerable<IEntity>> GetBlock(IEnumerator<IEntity> data, int records = 50000)
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

            try
            {
                foreach (IEntity obj in data)
                {
                    if (cmd[0] == null)
                    {
                        EntityType type = obj.EntityType;
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

                        cmd[0] = new SqliteCommand(String.Format("INSERT INTO [_{0}]({1}) VALUES({2})", tableName, fNames, fParameters), ActiveConnection, tran);
                        cmd[1] = new SqliteCommand(String.Format("UPDATE [_{0}] SET {1} WHERE [Id] = @Id", tableName, fFields), ActiveConnection, tran);
                        foreach (string column in columns)
                        {
                            cmd[0].Parameters.Add("@" + column, _supportedTypes[GetType(type.GetPropertyType(column))].DbType);
                            cmd[1].Parameters.Add("@" + column, _supportedTypes[GetType(type.GetPropertyType(column))].DbType);
                        }
                        cmd[0].Parameters.Add("@IsTombstone", System.Data.DbType.Boolean);
                        cmd[0].Parameters.Add("@IsDirty", System.Data.DbType.Boolean);
                        cmd[1].Parameters.Add("@IsTombstone", System.Data.DbType.Boolean);
                        cmd[1].Parameters.Add("@IsDirty", System.Data.DbType.Boolean);

                        cmd[2] = new SqliteCommand(String.Format("SELECT Id FROM [_{0}] WHERE [Id] = @Id", tableName), ActiveConnection, tran);
                        cmd[2].Parameters.Add("@Id", System.Data.DbType.String);

                        cmd[3] = new SqliteCommand(String.Format("DELETE FROM [_{0}] WHERE [Id] = @Id", tableName), ActiveConnection, tran);
                        cmd[3].Parameters.Add("@Id", System.Data.DbType.String);
                    }

                    //row id
                    DbRef id;
                    if (mode == ProcessMode.InitialLoad || mode == ProcessMode.ServerChanges)
                        id = DbRef.CreateInstance(tableName, ((ISqliteEntity)obj).EntityId);
                    else
                        id = DbRef.FromString(obj.GetValue(columns[0]).ToString());

                    int idx = 0; //insert 
                    if (mode != ProcessMode.InitialLoad)
                    {
                        if (((ISqliteEntity)obj).IsTombstone)
                        {
                            idx = mode == ProcessMode.ServerChanges ? 3 : 1;
                        }
                        else
                        {
                            cmd[2].Parameters[0].Value = id;
                            if (cmd[2].ExecuteScalar() != null)
                                idx = 1; //update
                        }
                    }

                    //assign values
                    if (idx > 2) //delete
                    {
                        cmd[idx].Parameters[0].Value = id;
                    }
                    else
                    {
                        int n = 0;
                        foreach (string column in columns)
                        {
                            cmd[idx].Parameters[n].Value = obj.GetValue(column);
                            n++;
                        }
                        cmd[idx].Parameters[n].Value = ((ISqliteEntity)obj).IsTombstone ? 1 : 0; //IsTombstone
                        var entity = (ISqliteEntity)obj;
                        cmd[idx].Parameters[n + 1].Value =
                            mode == ProcessMode.LocalChanges && (entity.IsNew() || entity.IsModified())
                            ? 1 : 0;  //isDirty
                    }

                    if (mode == ProcessMode.LocalChanges && inTran)
                        CopyTranObject(obj, tran, tableName, id.ToString(), idx);

                    cmd[idx].ExecuteNonQuery();
                }
            }
            finally
            {
                foreach (var c in cmd)
                {
                    c.Dispose();
                }
            }

            foreach (SqliteCommand c in cmd)
            {
                if (c != null)
                    c.Dispose();
            }
        }

        private string[] GetColumns(EntityType type)
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
            SqliteTransaction tran = ActiveConnection.BeginTransaction();
            try
            {
                using (var cmd = new SqliteCommand(ActiveConnection))
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = String.Format("DELETE FROM {0}", AnchorTable);
                    cmd.ExecuteNonQuery();

                    if (anchor != null)
                    {
                        String data = Convert.ToBase64String(anchor);
                        cmd.CommandText = String.Format("INSERT INTO {0}([Data]) VALUES(@Data)", AnchorTable);
                        cmd.Parameters.AddWithValue("@Data", data);
                        cmd.ExecuteNonQuery();
                    }
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public byte[] LoadAnchor()
        {
            using (var cmd = new SqliteCommand(String.Format("SELECT Data FROM {0} LIMIT 1", AnchorTable), ActiveConnection))
            {
                object data = cmd.ExecuteScalar();
                if (data == DBNull.Value)
                    return null;
                return Convert.FromBase64String(data.ToString());
            }
        }

        public void InitialLoadComplete(String userId, string userEmail)
        {
            UpdateDbStatus(userId, true);
            UpdateUserInfo(userId, userEmail);
            CloseConnection();
        }

        public void SyncComplete(bool success)
        {
            if (success)
            {
                SqliteTransaction tran = ActiveConnection.BeginTransaction();
                try
                {
                    CreateSchema();
                    foreach (String tableName in _tablesByIdx)
                    {
                        if (tableName.StartsWith("_Catalog_") || tableName.StartsWith("_Document_"))
                        {
                            using (var cmd = new SqliteCommand(String.Format("UPDATE {0} SET IsDirty = 0 WHERE IsDirty = 1", tableName), ActiveConnection, tran))
                                cmd.ExecuteNonQuery();
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
            string query = String.Format("INSERT OR REPLACE INTO {0}([UserId],[{1}]) VALUES(@UserId,@LastSync)"
                , DbStatusTable, SuccessSync ? "LastSync" : "LastSyncError");
            using (var cmd = new SqliteCommand(query, ActiveConnection))
            {
                DateTime ls = DateTime.Now;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@LastSync", ls);
                cmd.ExecuteNonQuery();

                _userId = userId;
                _lastSync = ls;
                _isSynced = true;
            }
        }

        void UpdateUserInfo(String userId, string email)
        {
            string query = String.Format(
                "INSERT OR REPLACE INTO {0}([UserId],[Email]) VALUES(@UserId,@Email)", DbUserInfo);
            using (var cmd = new SqliteCommand(query, ActiveConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.ExecuteNonQuery();
            }

            UserEmail = email;
        }
    }

    public class DbColumn
    {
        public String TypeName { get; set; }
        public System.Data.DbType DbType { get; set; }
    }
}
