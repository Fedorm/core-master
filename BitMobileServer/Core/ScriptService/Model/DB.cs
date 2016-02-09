using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ScriptService
{
    public class DB
    {
        private String connectionString;

        public DB(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public Query CreateCommand()
        {
            return new Query(connectionString);
        }
    }

    public class Query
    {
        private String connectionString;
        private Dictionary<String,object> parameters;
        private String text;

        public String Text
        {
            get { return text; }
            set { text = value; }
        }

        public Query(String connectionString)
        {
            this.connectionString = connectionString;
            parameters = new Dictionary<string, object>();
        }

        public void AddParameter(String name, object value)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (parameters.ContainsKey(name))
                {
                    parameters.Remove(name);
                }
                parameters.Add(name, value);
            }
        }

        public void Execute(String text)
        {
            this.text = text;
            Execute();
        }

        public void Execute()
        {
            if (!String.IsNullOrEmpty(text))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(text, conn);
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IDbRecordset Select()
        {
            if (!String.IsNullOrEmpty(text))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(text, conn);
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }

                    SqlDataAdapter a = new SqlDataAdapter(cmd);
                    DataTable t = new DataTable("recordset");
                    a.Fill(t);

                    return new DbRecordset(t);
                }
            }
            else
                return null;
        }

        public object ExecuteScalar()
        {
            if (!String.IsNullOrEmpty(text))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(text, conn);
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }

                    return cmd.ExecuteScalar();
                }
            }
            else
                return null;
        }

    }
}
