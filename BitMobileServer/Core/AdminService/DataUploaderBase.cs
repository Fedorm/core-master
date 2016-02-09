using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService
{
    public abstract class DataUploaderBase
    {
        public abstract void UploadData(Common.Solution solution, Stream messageBody, bool checkExisting);

        protected void UpdateCurrentCulterInfo()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("ru-RU");
        }

        protected SqlConnection GetConnection(Common.Solution solution)
        {
            SqlConnection conn = new SqlConnection(solution.ConnectionString);
            conn.Open();
            return conn;
        }

        public void SetRegionalSettings(Dictionary<String, String> settings)
        {
            foreach (var s in settings)
            {
                if (s.Key != null)
                {
                    switch (s.Key.ToLower())
                    {
                        case "numbergroupseparator":
                            System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = s.Value;
                            break;
                        case "numberdecimalseparator":
                            System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = s.Value;
                            break;
                    }
                }
            }
        }
    }
}
