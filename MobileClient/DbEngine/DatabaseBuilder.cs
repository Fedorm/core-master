using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Application.IO;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public partial class Database
    {
        private const string AnchorTable = "___Anchor";
        private const string TranStatusTable = "___TranStatus";
        private const string DbStatusTable = "___DbStatus";
        private const string DbUserInfo = "___UserInfo";
        private const string SolutionInfoTable = "___SolutionInfo";
        private Dictionary<Type, DbColumn> _supportedTypes;
        private Dictionary<String, int> _idxByTable;
        private string[] _tablesByIdx;

        public string LocationsTable
        {
            get { return "___DbLocations"; }
        }

        public string LogTable
        {
            get { return "___Log"; }
        }

        private static void AddSupportedTypes()
        {
            AddTypes();
        }

        private static void AddTypes()
        {
            _db._supportedTypes = new Dictionary<Type, DbColumn>
            {
                {
                    typeof (DbRef), new DbColumn
                    {
                        TypeName = "TEXT",
                        DbType = System.Data.DbType.String
                    }
                },
                {
                    typeof (IDbRef), new DbColumn
                    {
                        TypeName = "TEXT",
                        DbType = System.Data.DbType.String
                    }
                },
                {
                    typeof (Guid), new DbColumn
                    {
                        TypeName = "TEXT",
                        DbType = System.Data.DbType.String
                    }
                },
                {
                    typeof (int), new DbColumn
                    {
                        TypeName = "INTEGER",
                        DbType = System.Data.DbType.Int32
                    }
                },
                {
                    typeof (bool), new DbColumn
                    {
                        TypeName = "INTEGER",
                        DbType = System.Data.DbType.Int32
                    }
                },
                {
                    typeof (Double), new DbColumn
                    {
                        TypeName = "REAL",
                        DbType = System.Data.DbType.Double
                    }
                },
                {
                    typeof (Decimal), new DbColumn
                    {
                        TypeName = "REAL",
                        DbType = System.Data.DbType.Double
                    }
                },
                {
                    typeof (String), new DbColumn
                    {
                        TypeName = "TEXT",
                        DbType = System.Data.DbType.String
                    }
                },
                {
                    typeof (DateTime), new DbColumn
                    {
                        TypeName = "TEXT",
                        DbType = System.Data.DbType.DateTime
                    }
                }
            };
        }

        private static void InitFunctions()
        {
            DbFunctions.Init(_db._connection);
        }

        private static bool DbExists()
        {
            return IOContext.Current.Exists(GetDbPath());
        }

        public static void CreateDatabase()
        {
            DeleteDatabase();
            SqliteConnection.CreateFile(GetDbPath());
            _db.CreateSystemTables();
        }

        private static void DeleteDatabase()
        {
            if (_db != null)
            {
                _db.CloseConnection();
            }
            String path = GetDbPath();
            IOContext.Current.Delete(path);
            IOContext.Current.Delete(path + "-shm");
            IOContext.Current.Delete(path + "-wal");
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

        private void CreateSystemTables()
        {
            CreateDbStatusTable();
            CreateAnchorTable();
            CreateTranStatusTable();
            CreateLocationsTable();
            CreateUserInfoTable();
            CreateLogTable();
            CreateSolutionInfo();
        }

        public void CreateEntityTables(IEnumerable<IEntityType> types)
        {
            foreach (IEntityType t in types)
            {
                if (t.IsTable)
                    CreateTable(t);
            }
        }

        private void CreateTable(IEntityType type)
        {
            String tableName = type.TableName;
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
            NonQueryScript("CREATE TABLE {0}([Data] TEXT)", AnchorTable);
        }

        private void CreateTranStatusTable()
        {
            NonQueryScript("CREATE TABLE {0}([Id] TEXT PRIMARY KEY, [TableName] TEXT, [Status] INTEGER)",
                TranStatusTable);
        }

        private void CreateUserInfoTable()
        {
            NonQueryScript("CREATE TABLE {0}([UserId] TEXT PRIMARY KEY, [Email] TEXT)", DbUserInfo);
        }

        private void CreateDbStatusTable()
        {
            NonQueryScript("CREATE TABLE {0}([UserId] TEXT PRIMARY KEY, [LastSync] TEXT, [LastSyncError] TEXT)",
                DbStatusTable);
        }

        private void CreateLocationsTable()
        {
            NonQueryScript("CREATE TABLE {0}([Id] TEXT PRIMARY KEY, [Latitude] REAL, [Longitude] REAL, [BeginTime] TEXT, [EndTime] TEXT, [Speed] REAL, [Direction] REAL, [SatellitesCount] INTEGER, [Altitude] REAL)"
                , LocationsTable);
        }

        private void CreateLogTable()
        {
            NonQueryScript("CREATE TABLE {0}([Date] TEXT, [Event] TEXT, [Content] TEXT)", LogTable);
            NonQueryScript("CREATE INDEX inx_date ON {0}([Date])", LogTable);
        }

        private void CreateSolutionInfo()
        {
            NonQueryScript("CREATE TABLE {0}([Key] TEXT PRIMARY KEY, [Value] TEXT)", SolutionInfoTable);
        }

        private void NonQueryScript(string query, params object[] p)
        {
            using (var cmd = new SqliteCommand(string.Format(query, p), ActiveConnection))
                cmd.ExecuteNonQuery();
        }
    }
}