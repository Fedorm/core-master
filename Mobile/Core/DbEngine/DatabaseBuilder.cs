using System;
using System.Collections.Generic;
using BitMobile.Common.Entites;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database
    {
        public const String LocationsTable = "___DbLocations";
        private const String AnchorTable = "___Anchor";
        private const String TranStatusTable = "___TranStatus";
        private const String DbStatusTable = "___DbStatus";
        private const String DbUserInfo = "___UserInfo";
        private Dictionary<Type, DbColumn> _supportedTypes;
        private Dictionary<String, int> _idxByTable;
        private String[] _tablesByIdx;

        private static void AddSupportedTypes()
        {
            AddTypes();
        }

        private static void AddTypes()
        {
            _db._supportedTypes = new Dictionary<Type, DbColumn>();
            _db._supportedTypes.Add(typeof(DbRef), new DbColumn
            {
                TypeName = "TEXT",
                DbType = System.Data.DbType.String
            });
            _db._supportedTypes.Add(typeof(IDbRef), new DbColumn
            {
                TypeName = "TEXT",
                DbType = System.Data.DbType.String
            });
            _db._supportedTypes.Add(typeof(Guid), new DbColumn
            {
                TypeName = "TEXT",
                DbType = System.Data.DbType.String
            });
            _db._supportedTypes.Add(typeof(int), new DbColumn
            {
                TypeName = "INTEGER",
                DbType = System.Data.DbType.Int32
            });
            _db._supportedTypes.Add(typeof(bool), new DbColumn
            {
                TypeName = "INTEGER",
                DbType = System.Data.DbType.Int32
            });
            _db._supportedTypes.Add(typeof(Double), new DbColumn
            {
                TypeName = "REAL",
                DbType = System.Data.DbType.Double
            });
            _db._supportedTypes.Add(typeof(Decimal), new DbColumn
            {
                TypeName = "REAL",
                DbType = System.Data.DbType.Double
            });
            _db._supportedTypes.Add(typeof(String), new DbColumn
            {
                TypeName = "TEXT",
                DbType = System.Data.DbType.String
            });
            _db._supportedTypes.Add(typeof(DateTime), new DbColumn
            {
                TypeName = "TEXT",
                DbType = System.Data.DbType.DateTime
            });
        }

        private static void InitFunctions()
        {
            DbFunctions.Init(_db._connection);
        }

        private static bool DbExists()
        {
            return System.IO.File.Exists(GetDbPath());
        }

        public static void CreateDatabase()
        {
            DeleteDatabase();
            SqliteConnection.CreateFile(GetDbPath());
            _db.CreateSystemTables();
        }

        public static void DeleteDatabase()
        {
            _db.CloseConnection();
            String path = GetDbPath();
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private void CreateSchema()
        {
            if (_idxByTable == null)
            {
                System.Data.DataTable tbl = ActiveConnection.GetSchema(SqliteMetaDataCollectionNames.Tables);
                if (tbl.Rows.Count == 0)
                    return;

                _tablesByIdx = new string[tbl.Rows.Count];
                int idx = 0;
                foreach (System.Data.DataRow row in tbl.Rows)
                {
                    _tablesByIdx[idx] = row[2].ToString();
                    idx++;
                }
                Array.Sort(_tablesByIdx);

                _idxByTable = new Dictionary<string, int>();
                idx = 0;
                foreach (String s in _tablesByIdx)
                {
                    _idxByTable.Add(s, idx);
                    idx++;
                }
            }
        }

        public void CreateSystemTables()
        {
            CreateDbStatusTable();
            CreateAnchorTable();
            CreateTranStatusTable();
            CreateLocationsTable();
            CreateUserInfoTable();
        }

        public void CreateEntityTables(EntityType[] types)
        {
            foreach (EntityType t in types)
            {
                if (t.IsTable)
                    CreateTable(t);
            }
        }

        public void CreateTable(EntityType type)
        {
            //String[] arr = type.Name.Split('.');
            String tableName = type.TableName;//String.Format("{0}_{1}", arr[arr.Length - 2], arr[arr.Length - 1]);
            String columns = "";
            String columnsOnly = "";

            var indexedFields = new List<string>();

            foreach (string column in type.GetColumns())
            {
                Type columnType = GetType(type.GetPropertyType(column));
                if (!_supportedTypes.ContainsKey(columnType))
                    throw new Exception(String.Format("Unsupported column type '{0}'", columnType));

                String columnName = String.Format("{0}", column);
                String typeDeclaration = String.Format(_supportedTypes[columnType].TypeName);

                String primaryKey = type.IsPrimaryKey(column) ? "PRIMARY KEY" : "";

                const string notNull = "";
                
                if (type.IsIndexed(column))
                    indexedFields.Add(columnName);

                String s = String.Format("[{0}] {1} {2} {3}", columnName, typeDeclaration, notNull, primaryKey);
                String s1 = String.Format("[{0}]", columnName);

                if (!String.IsNullOrEmpty(columns))
                {
                    s = "," + s;
                    s1 = "," + s1;
                }

                columns = columns + s;
                columnsOnly = columnsOnly + s1;
            }

            columns += ",[IsTombstone] INTEGER,[IsDirty] INTEGER";
            columnsOnly += ",[IsDirty]";

            //create table
            using (var cmd = new SqliteCommand(String.Format("CREATE TABLE [_{0}] ({1})", tableName, columns), ActiveConnection))
                cmd.ExecuteNonQuery();

            //create tran table
            using (var cmd = new SqliteCommand(String.Format("CREATE TABLE [__{0}] ({1})", tableName, columns), ActiveConnection))
                cmd.ExecuteNonQuery();

            //create indexes
            indexedFields.Add("IsTombstone");
            foreach (String idx in indexedFields)
            {
                String idxScript = String.Format("CREATE INDEX [{0}_{1}] ON _{0}([{1}])", tableName, idx);
                using (var idxCmd = new SqliteCommand(idxScript, ActiveConnection))
                    idxCmd.ExecuteNonQuery();
            }

            //create view
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE VIEW [{0}] AS SELECT {1} FROM [_{0}] WHERE IsTombstone = 0", tableName, columnsOnly), ActiveConnection))
                cmd.ExecuteNonQuery();
        }

        private void CreateAnchorTable()
        {
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE TABLE {0}([Data] TEXT)", AnchorTable), ActiveConnection))
                cmd.ExecuteNonQuery();
        }

        private void CreateTranStatusTable()
        {
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE TABLE {0}([Id] TEXT PRIMARY KEY, [TableName] TEXT, [Status] INTEGER)", TranStatusTable), ActiveConnection))
                cmd.ExecuteNonQuery();
        }

        private void CreateUserInfoTable()
        {
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE TABLE {0}([UserId] TEXT PRIMARY KEY, [Email] TEXT)", DbUserInfo), ActiveConnection))
                cmd.ExecuteNonQuery();
        }

        private void CreateDbStatusTable()
        {
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE TABLE {0}([UserId] TEXT PRIMARY KEY, [LastSync] TEXT, [LastSyncError] TEXT)", DbStatusTable), ActiveConnection))
                cmd.ExecuteNonQuery();
        }

        private void CreateLocationsTable()
        {
            using (var cmd = new SqliteCommand(String.Format(
                "CREATE TABLE {0}([Id] TEXT PRIMARY KEY, [Latitude] REAL, [Longitude] REAL, [BeginTime] TEXT, [EndTime] TEXT, [Speed] REAL, [Direction] REAL, [SatellitesCount] INTEGER, [Altitude] REAL)", LocationsTable), ActiveConnection))
                cmd.ExecuteNonQuery();
        }
    }
}