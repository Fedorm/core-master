using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory
{
    public static class KeysPatcher
    {
        public static List<KeyInfo> GetClusteredKeys(Config config, String connectionString)
        {
            return GetKeysInternal(config, connectionString, "CLUSTERED");
        }

        public static List<KeyInfo> GetNonClusteredKeys(Config config, String connectionString)
        {
            return GetKeysInternal(config, connectionString, "NONCLUSTERED");
        }    

        private static List<KeyInfo> GetKeysInternal(Config config, String connectionString, String keyType)
        {
            Dictionary<String, KeyInfo> keys = new Dictionary<string, KeyInfo>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(String.Format("select s.name, o.name, i.name from sys.objects o join sys.indexes i on i.object_id = o.object_id join sys.schemas s on s.schema_id = o.schema_id where i.type_desc = '{0}' and i.is_primary_key = 1 and o.type = 'U' order by 1", keyType), conn);
                using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String schemaName = r[0].ToString();
                        String tableName = r[1].ToString();
                        String keyName = r[2].ToString();
                        keys.Add((schemaName + tableName).ToLower(), new KeyInfo() { Name = keyName, SchemaName = schemaName, TableName = tableName });
                    }
                }
            }

            Dictionary<String, List<Entity>> schemas = config.EntitiesBySchema;
            List<KeyInfo> result = new List<KeyInfo>();
            foreach (KeyValuePair<String, KeyInfo> kvp in keys)
            {
                if (schemas.ContainsKey(kvp.Value.SchemaName))
                    result.Add(kvp.Value);
            }

            return result;
        }
    }

    public class KeyInfo
    {
        public String SchemaName { get; set; }
        public String TableName { get; set; }
        public String Name { get; set; }
    }
}
