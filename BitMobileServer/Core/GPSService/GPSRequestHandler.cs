using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.ServiceModel.Web;

namespace GPSService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GPSService : IGPSRequestHandler
    {
        private String scope;
        private System.Net.NetworkCredential credential;

        public GPSService(String scope, System.Net.NetworkCredential credential)
        {
            this.credential = credential;
            this.scope = scope;
        }

        public System.IO.Stream PostData(System.IO.Stream messageBody)
        {
            System.Data.DataTable tbl = new System.Data.DataTable();
            tbl.ReadXml(messageBody);
            if (tbl.Rows.Count == 0)
                return MakeTextAnswer("ok");
            else
            {
                Common.Solution solution = Common.Solution.CreateFromContext(this.scope);

                DateTime serverTime = DateTime.UtcNow;
                Guid userId = Guid.Parse(WebOperationContext.Current.IncomingRequest.Headers["userid"]);

                using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmd = new SqlCommand("INSERT INTO [admin].GPS([UserId],[ServerTime],[BeginTime],[EndTime],[Latitude],[Longitude],[Speed],[Direction],[SatellitesCount]) VALUES(@UserId,@ServerTime,@BeginTime,@EndTime,@Latitude,@Longitude,@Speed,@Direction,@SatellitesCount)", conn, tran);
                        cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier);
                        cmd.Parameters.Add("@ServerTime", SqlDbType.DateTime);
                        cmd.Parameters.Add("@BeginTime", SqlDbType.DateTime);
                        cmd.Parameters.Add("@EndTime", SqlDbType.DateTime);
                        cmd.Parameters.Add("@Latitude", SqlDbType.Decimal);
                        cmd.Parameters.Add("@Longitude", SqlDbType.Decimal);
                        cmd.Parameters.Add("@Speed", SqlDbType.Decimal);
                        cmd.Parameters.Add("@Direction", SqlDbType.Int);
                        cmd.Parameters.Add("@SatellitesCount", SqlDbType.Int);
                        cmd.Parameters.Add("@Altitude", SqlDbType.Decimal);
                        foreach (DataRow row in tbl.Rows)
                        {
                            cmd.Parameters["@UserId"].Value = userId;
                            cmd.Parameters["@ServerTime"].Value = serverTime;
                            cmd.Parameters["@BeginTime"].Value = row["BeginTime"];
                            cmd.Parameters["@EndTime"].Value = row["EndTime"];
                            cmd.Parameters["@Latitude"].Value = row["Latitude"];
                            cmd.Parameters["@Longitude"].Value = row["Longitude"];
                            cmd.Parameters["@Speed"].Value = row["Speed"];
                            cmd.Parameters["@Direction"].Value = row["Direction"];
                            cmd.Parameters["@SatellitesCount"].Value = row["SatellitesCount"];
                            cmd.Parameters["@Altitude"].Value = row["Altitude"];
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        return MakeTextAnswer("ok");
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private System.IO.Stream MakeTextAnswer(String text)
        {
            MemoryStream ms = new MemoryStream();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return ms;
        }
    }
}
