using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AdminService
{
    public class DataUploader : DataUploaderBase
    {
        public override void UploadData(Common.Solution solution, Stream messageBody, bool checkExisting)
        {
            UpdateCurrentCulterInfo();

            SqlConnection conn = GetConnection(solution);
            using (conn)
            {
                SqlTransaction tran = conn.BeginTransaction();
                Dictionary<String, SqlCommand[]> sqlCommands = new Dictionary<String, SqlCommand[]>();
                Dictionary<XmlNode, Exception> fkErrors = new System.Collections.Generic.Dictionary<XmlNode, Exception>();

                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(messageBody);

                    XmlNodeList rows = doc.DocumentElement.SelectNodes("//Root/Rows/Row");
                    foreach (XmlNode row in rows)
                    {
                        UploadRow(row, tran, sqlCommands, fkErrors, checkExisting);
                    }

                    int pass = 0;
                    int cnt = 0;
                    System.Collections.IEnumerable errorRows = null;
                    while (fkErrors.Count > 0)
                    {
                        if (pass > 0 && cnt == 0 && fkErrors.Count > 0)
                        {
                            System.Collections.IEnumerator err = fkErrors.Values.GetEnumerator();
                            if (err.MoveNext())
                                throw (Exception)err.Current;
                        }

                        cnt = 0;
                        errorRows = new List<XmlNode>(fkErrors.Keys);
                        foreach (XmlNode row in errorRows)
                        {
                            if (UploadRow(row, tran, sqlCommands, fkErrors, checkExisting))
                            {
                                fkErrors.Remove(row);
                                cnt++;
                            }
                        }

                        pass++;
                    }
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }


        private bool UploadRow(XmlNode row, SqlTransaction tran, Dictionary<String, SqlCommand[]> sqlCommands, Dictionary<XmlNode, Exception> fkErrors, bool checkExisting = false)
        {
            String rawEntityName = row.Attributes["_Type"].Value;
            int cmdType = int.Parse(row.Attributes["_RS"].Value);
            String[] arr = rawEntityName.Split('.');
            String entityName = String.Format("[{0}].[{1}]", arr[0], arr[1]);
            SqlCommand[] cmds = null;

            if (!sqlCommands.ContainsKey(entityName))
            {
                cmds = new SqlCommand[5];
                cmds[0] = new SqlCommand(String.Format("[{0}].[{1}_adm_insert]", arr[0], arr[1]), tran.Connection, tran);
                cmds[1] = new SqlCommand(String.Format("[{0}].[{1}_adm_update]", arr[0], arr[1]), tran.Connection, tran);
                cmds[2] = new SqlCommand(String.Format("[{0}].[{1}_adm_delete]", arr[0], arr[1]), tran.Connection, tran);
                cmds[3] = new SqlCommand(String.Format("[{0}].[{1}_adm_markdelete]", arr[0], arr[1]), tran.Connection, tran);
                cmds[4] = null;
                for (int i = 0; i < 4; i++)
                {
                    SqlCommand c = cmds[i];
                    c.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(c);
                }
                sqlCommands.Add(entityName, cmds);

                if (checkExisting)
                {
                    SqlCommand c = new SqlCommand(String.Format("[{0}].[{1}_adm_exists]", arr[0], arr[1]), tran.Connection, tran);
                    c.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        SqlCommandBuilder.DeriveParameters(c);
                        cmds[4] = c;
                    }
                    catch
                    {
                    }
                }

            }
            else
                cmds = sqlCommands[entityName];

            SqlCommand cmd = cmds[cmdType];
            FillParameters(cmd, row);

            if (checkExisting && cmds[4] != null && (cmdType == 0 || cmdType == 1))
            {
                cmds[4].Parameters["@Id"].Value = Guid.Parse(row.Attributes["Id"].Value);
                object v = cmds[4].ExecuteScalar();
                if (v != null && cmdType == 0) //exists, will do update instead
                {
                    cmdType = 1;
                    cmd = cmds[cmdType];
                    FillParameters(cmd, row);
                }
                if (v == null && cmdType == 1) //not exists, will do insert instead
                {
                    cmdType = 0;
                    cmd = cmds[cmdType];
                    FillParameters(cmd, row);
                }
            }

            try
            {
                cmd.ExecuteNonQuery();

                if (cmdType == 0)
                    row.Attributes["_RS"].Value = "1";

                if (cmdType != 2) //insert, update or markdelete
                {
                    foreach (XmlNode tabularSection in row.ChildNodes)
                    {
                        SqlCommand childCmd = null;
                        String ts = String.Format("{0}_{1}", rawEntityName, tabularSection.Name);

                        if (cmdType > 0) //update or markdelete
                        {
                            childCmd = new SqlCommand(String.Format("{0}_adm_clear", ts), tran.Connection, tran);
                            childCmd.CommandType = CommandType.StoredProcedure;
                            SqlCommandBuilder.DeriveParameters(childCmd);
                            childCmd.Parameters["@Ref"].Value = cmd.Parameters["@Id"].Value;
                            childCmd.ExecuteNonQuery();
                        }

                        childCmd = new SqlCommand(String.Format("{0}_adm_insert", ts), tran.Connection, tran);
                        childCmd.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(childCmd);

                        foreach (XmlNode childRow in tabularSection.SelectNodes("Row"))
                        {
                            childCmd = FillParameters(childCmd, childRow);
                            childCmd.ExecuteNonQuery();
                        }

                    }
                }
                return true;
            }
            catch (SqlException e)
            {
                if (e.Message.ToLower().Contains("foreign key") || e.Message.ToLower().Contains("reference constraint"))
                {
                    if (!fkErrors.ContainsKey(row))
                        fkErrors.Add(row, new Exception(String.Format("Key violation at object '{0}'", row.Attributes["Id"].Value), e));
                    return false;
                }
                else
                    throw;
            }
        }

        private SqlCommand FillParameters(SqlCommand cmd, XmlNode row)
        {
            for (int i = 1; i < cmd.Parameters.Count; i++)
            {
                SqlParameter param = cmd.Parameters[i];
                XmlAttribute a = row.Attributes[param.ParameterName.Substring(1)];
                if (a != null)
                {
                    String value = a.Value;
                    if (value.ToLower().Equals("null"))
                        param.Value = DBNull.Value;
                    else
                    {
                        if (param.DbType == DbType.Guid)
                        {
                            if (value.Equals("00000000-0000-0000-0000-000000000000"))
                                param.Value = DBNull.Value;
                            else
                                param.Value = new Guid(value);
                        }
                        else
                            param.Value = value;
                    }
                }
            }

            return cmd;
        }


    }
}
