using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory
{
    public static class SyncOrder
    {
        private static SyncOrderInternal impl;

        public static void Initialize(String connectionString)
        {
            if (impl == null)
                impl = new SyncOrderInternal(connectionString);
        }

        public static void Sort(List<Entity> entities)
        {
            if (impl != null)
                entities.Sort(impl);
        }
    }

    public class SyncOrderInternal : IComparer<Entity>
    {
        private List<String> syncOrder;
        public List<String> SyncOrder
        {
            get
            {
                return syncOrder;
            }
        }

        public SyncOrderInternal(String connectionString)
        {
            syncOrder = ReArrange(GetMetadata(connectionString));
        }

        private List<String> ReArrange(Dictionary<String, List<String>> links)
        {
            foreach (var kvp in links)
                kvp.Value.Remove(kvp.Key);

            List<string> list = links.Keys.ToList();

            int cnt = 0;
            bool f;
            do
            {
                f = false;
                List<String> tl = new List<string>();
                for (int i = 0; i < list.Count - 1; i++)
                {
                    String key = list[i];
                    if (links.ContainsKey(key))
                    {
                        foreach (String s in links[key])
                        {
                            if (links.ContainsKey(s))
                            {
                                if (!tl.Contains(s))
                                {
                                    list.Remove(s);
                                    list.Insert(i, s);
                                    tl.Add(s);
                                    f = true;
                                }
                            }
                        }
                        tl.Add(key);
                    }
                }

                cnt++;
                if (cnt > 1000)
                    throw new Exception("Bad metadata. Circular references found");
            }
            while (f);

            return list;
        }

        private Dictionary<String, List<String>> GetMetadata(String connectionString)
        {
            Dictionary<String, List<String>> result = new Dictionary<string, List<String>>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT SCHEMA_NAME(t.schema_id) + '_' + OBJECT_NAME(t.object_id), SCHEMA_NAME(fk.schema_id) + '_' + OBJECT_NAME(fk.referenced_object_id) FROM sys.tables t LEFT JOIN sys.foreign_keys fk on fk.parent_object_id = t.object_id ORDER BY 1,2", conn);
                using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String key = r[0].ToString();
                        String value = r[1].ToString();
                        if (!result.ContainsKey(key))
                            result.Add(key, new List<String>());
                        if (!String.IsNullOrEmpty(value))
                            result[key].Add(value);
                    }
                }
            }
            return result;
        }

        public int Compare(Entity x, Entity y)
        {
            return syncOrder.IndexOf(String.Format("{0}_{1}", x.Schema, x.Name)).CompareTo(
                syncOrder.IndexOf(String.Format("{0}_{1}", y.Schema, y.Name)));
        }
    }
}
