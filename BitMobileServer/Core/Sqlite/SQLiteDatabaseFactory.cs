using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.DatabaseFactory
{
    public class SQLiteDatabaseFactory
    {
        private String fileName;

        public SQLiteDatabaseFactory(String fileName)
        {
            this.fileName = fileName;
        }

        public void CreateDatabase()
        {
            SQLiteConnection.CreateFile(fileName);
        }

        public void RunScript(String script, bool inTransaction = false)
        {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data source={0}", fileName));
            conn.Open();
            using (conn)
            {
                SQLiteTransaction tran = null;
                if (inTransaction)
                    conn.BeginTransaction();
                try
                {
                    String[] commands = script.Split(new String[] { "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String command in commands)
                    {
                        String s = command;//.Replace("\r\n", "");
                        if (!String.IsNullOrEmpty(s))
                        {
                            new SQLiteCommand(s, conn, tran).ExecuteNonQuery();
                        }
                    }
                    if (inTransaction)
                        tran.Commit();
                }
                catch (Exception e)
                {
                    if (inTransaction)
                        tran.Rollback();
                    throw e;
                }

            }
        }

        public void InsertMetadata(String configurationFile)
        {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data source={0}", fileName));
            conn.Open();
            using (conn)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO ___Metadata([Data]) VALUES(@Data)", conn))
                {
                    SQLiteParameter p = cmd.CreateParameter();
                    p.ParameterName = "@Data";
                    p.DbType = System.Data.DbType.Xml;
                    p.Value = System.IO.File.ReadAllText(configurationFile);
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
