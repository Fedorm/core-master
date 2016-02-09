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
    public class DataUploader2 : DataUploaderBase
    {
        private Action<int> progressCallback;
        int progressStep;

        private DataUploader2()
        {
        }

        public DataUploader2(Action<int> progressCallback = null, int progressStep = 0)
        {
            this.progressCallback = progressCallback;
            this.progressStep = progressStep;
        }

        public override void UploadData(Common.Solution solution, Stream messageBody, bool checkExisting)
        {
            UpdateCurrentCulterInfo();

            int progressCnt = 0;

            SqlConnection conn = GetConnection(solution);
            using (conn)
            {
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    Dictionary<String, SqlCommand[]> sqlCommands = new Dictionary<String, SqlCommand[]>();
                    Dictionary<Entity, Exception> fkErrors = new System.Collections.Generic.Dictionary<Entity, Exception>();

                    try
                    {
                        System.Xml.XmlTextReader doc = new XmlTextReader(messageBody);
                        try
                        {
                            using (doc)
                            {
                                Dictionary<String, String> regionalSetting = FastXmlReader.ReadRegionalSettings(doc);
                                if (regionalSetting != null)
                                    base.SetRegionalSettings(regionalSetting);

                                if (progressCallback != null)
                                    progressCallback(0);

                                foreach (Entity row in FastXmlReader.ReadRow(doc))
                                {
                                    if (UploadRow(row, tran, sqlCommands, fkErrors, checkExisting) && progressCallback != null)
                                    {
                                        progressCnt++;
                                        if ((progressCnt % progressStep) == 0)
                                            progressCallback(progressCnt);
                                    }
                                }
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
                                errorRows = new List<Entity>(fkErrors.Keys);
                                foreach (Entity row in errorRows)
                                {
                                    if (UploadRow(row, tran, sqlCommands, fkErrors, checkExisting))
                                    {
                                        fkErrors.Remove(row);
                                        cnt++;

                                        if (progressCallback != null)
                                        {
                                            progressCnt++;
                                            if ((progressCnt % progressStep) == 0)
                                                progressCallback(progressCnt);
                                        }
                                    }
                                }

                                pass++;
                            }
                            tran.Commit();
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                tran.Rollback();
                            }
                            catch
                            {
                            }
                            throw e;
                        }
                    }
                    finally
                    {
                        try
                        {
                            if (sqlCommands != null)
                            {
                                foreach (KeyValuePair<String, SqlCommand[]> kvp in sqlCommands)
                                {
                                    SqlCommand[] arr = kvp.Value;
                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        if (arr[i] != null)
                                        {
                                            arr[i].Dispose();
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private bool UploadRow(Entity row, SqlTransaction tran, Dictionary<String, SqlCommand[]> sqlCommands, Dictionary<Entity, Exception> fkErrors, bool checkExisting = false)
        {
            String rawEntityName = row.Attributes["_Type"];
            int cmdType = int.Parse(row.Attributes["_RS"]);
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
                cmds[4].Parameters["@Id"].Value = Guid.Parse(row.Attributes["Id"]);
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
                    row.Attributes["_RS"] = "1";

                Guid entityId = (Guid)cmd.Parameters["@Id"].Value;

                if (cmdType != 2) //insert, update or markdelete
                {
                    foreach (var tabularSection in row.GetTabularSections())
                    {
                        TabularSection tSection = tabularSection.Value;

                        SqlCommand childCmd = null;
                        String ts = String.Format("{0}_{1}", rawEntityName, tabularSection.Key);

                        SqlCommand[] cmdsc = null;
                        if (!sqlCommands.ContainsKey(ts))
                        {
                            cmdsc = new SqlCommand[7];
                            cmdsc[0] = new SqlCommand(String.Format("{0}_adm_clear", ts), tran.Connection, tran);
                            cmdsc[1] = new SqlCommand(String.Format("{0}_adm_insert", ts), tran.Connection, tran);
                            if (!String.IsNullOrEmpty(tSection.Key))
                            {
                                cmdsc[2] = new SqlCommand(String.Format("{0}_adm_update", ts), tran.Connection, tran);
                                cmdsc[3] = new SqlCommand(String.Format("{0}_adm_delete", ts), tran.Connection, tran);
                                cmdsc[4] = new SqlCommand(String.Format("{0}_adm_selectkeys", ts), tran.Connection, tran);
                            }
                            for (int i = 0; i < cmdsc.Length; i++)
                            {
                                SqlCommand c = cmdsc[i];
                                if (c != null)
                                {
                                    c.CommandType = CommandType.StoredProcedure;
                                    SqlCommandBuilder.DeriveParameters(c);
                                }
                            }
                            //batch operations
                            try
                            {
                                SqlCommand bins = new SqlCommand(String.Format("{0}_adm_insert_batch", ts), tran.Connection, tran);
                                bins.CommandType = CommandType.StoredProcedure;
                                SqlCommandBuilder.DeriveParameters(bins);
                                String[] pn = bins.Parameters["@Data"].TypeName.Split('.');
                                bins.Parameters["@Data"].TypeName = String.Format("{0}.{1}", pn[pn.Length - 2], pn[pn.Length - 1]); //ignore db name
                                cmdsc[5] = bins;

                                SqlCommand bupd = new SqlCommand(String.Format("{0}_adm_update_batch_all", ts), tran.Connection, tran);
                                bupd.CommandType = CommandType.StoredProcedure;
                                SqlCommandBuilder.DeriveParameters(bupd);
                                String[] pn2 = bupd.Parameters["@Data"].TypeName.Split('.');
                                bupd.Parameters["@Data"].TypeName = String.Format("{0}.{1}", pn2[pn2.Length - 2], pn2[pn2.Length - 1]); //ignore db name
                                cmdsc[6] = bupd;
                            }
                            catch
                            {
                            }

                            sqlCommands.Add(ts, cmdsc);
                        }
                        cmdsc = sqlCommands[ts];

                        if (cmdType > 0) //update or markdelete
                        {
                            if (String.IsNullOrEmpty(tSection.Key) || tabularSection.Value.Entities.Count == 0)
                            {
                                //remove all
                                childCmd = cmdsc[0];
                                childCmd.Parameters["@Ref"].Value = entityId;
                                childCmd.ExecuteNonQuery();

                                //insert rows
                                if (tabularSection.Value.Entities.Count > 0)
                                    InsertTabularSection(entityId, tabularSection.Value, cmdsc[1], cmdsc[5]);
                            }
                            else
                            {
                                if (cmdsc[6] != null) //batch update is supported
                                {
                                    UpdateTabularSectionBatchAll(entityId, tabularSection.Value, cmdsc[1], cmdsc[6]);
                                }
                                else
                                {
                                    //select existing
                                    childCmd = cmdsc[4];
                                    childCmd.Parameters["@Ref"].Value = entityId;
                                    DataTable tbl = new DataTable();
                                    SqlDataAdapter ad = new SqlDataAdapter(childCmd);
                                    ad.Fill(tbl);

                                    String[] keyFields = tSection.KeyFields;
                                    Dictionary<String, String> keyValues = new Dictionary<String, String>();

                                    //update rows
                                    childCmd = cmdsc[2];
                                    foreach (var childRow in tabularSection.Value.Entities)
                                    {
                                        childCmd = FillParameters(childCmd, childRow);

                                        String stringKey = "";
                                        foreach (String kf in keyFields)
                                        {
                                            SqlParameter p = childCmd.Parameters["@" + kf];
                                            object keyValue = p.Value;
                                            if (p.DbType == System.Data.DbType.DateTime || p.DbType == System.Data.DbType.DateTime2)
                                                keyValue = System.DateTime.Parse(keyValue.ToString());

                                            if(keyValue is IFormattable)
                                                stringKey = stringKey + (keyValue == null ? "null" : ((IFormattable)keyValue).ToString(null, System.Globalization.CultureInfo.InvariantCulture)) + "_";
                                            else
                                                stringKey = stringKey + (keyValue == null ? "null" : keyValue.ToString()) + "_";
                                        }
                                        if (!keyValues.ContainsKey(stringKey))
                                            keyValues.Add(stringKey, "");

                                        childCmd.Parameters["@Ref"].Value = entityId;
                                        childCmd.ExecuteNonQuery();
                                    }

                                    //remove
                                    childCmd = cmdsc[3];
                                    foreach (DataRow r in tbl.Rows)
                                    {
                                        String stringKey = "";
                                        foreach (String kf in keyFields)
                                        {
                                            childCmd.Parameters["@" + kf].Value = r[kf];
                                            object keyValue = r[kf];
                                            if (keyValue is IFormattable)
                                                stringKey = stringKey + (keyValue == null ? "null" : ((IFormattable)keyValue).ToString(null, System.Globalization.CultureInfo.InvariantCulture)) + "_";
                                            else
                                                stringKey = stringKey + (keyValue == null ? "null" : keyValue.ToString()) + "_";
                                        }
                                        if (!keyValues.ContainsKey(stringKey))
                                        {
                                            childCmd.Parameters["@Ref"].Value = entityId;
                                            childCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //insert rows
                            InsertTabularSection(entityId, tabularSection.Value, cmdsc[1], cmdsc[5]);
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
                        fkErrors.Add(row, new Exception(String.Format("Key violation at object '{0}'", row.Attributes["Id"]), e));
                    return false;
                }
                else
                    throw;
            }
        }

        private void InsertTabularSection(Guid entityId, TabularSection section, SqlCommand childCmd, SqlCommand childCmdBatch)
        {
            if (childCmdBatch != null && section.Entities.Count > 1) //batch insert is supported
            {
                DataTable tbl = new DataTable();
                for (int i = 1; i < childCmd.Parameters.Count; i++)
                {
                    SqlParameter p = childCmd.Parameters[i];
                    String pName = p.ParameterName.Substring(1);
                    if (!pName.ToLower().Equals("ref"))
                        tbl.Columns.Add(pName, SqlDbTypeToType(p.SqlDbType));
                }

                FillDataTable(tbl, section, false);
                childCmdBatch.Parameters["@Ref"].Value = entityId;
                childCmdBatch.Parameters["@Data"].Value = tbl;
                childCmdBatch.ExecuteNonQuery();
            }
            else
            {
                foreach (var childRow in section.Entities)
                {
                    childCmd = FillParameters(childCmd, childRow);
                    childCmd.Parameters["@Ref"].Value = entityId;
                    childCmd.ExecuteNonQuery();
                }
            }

        }

        /*
        private void UpdateTabularSectionBatch(Guid entityId, TabularSection section, SqlCommand insCmd, SqlCommand selCmd, SqlCommand insCmdBatch, SqlCommand updCmdBatch, SqlCommand delCmdBatch)
        {
            String[] keyFields = section.KeyFields;

            //select existing
            selCmd.Parameters["@Ref"].Value = entityId;
            DataTable selTbl = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter(selCmd);
            ad.Fill(selTbl);

            Dictionary<String, DataRow> existingValues = new Dictionary<string, DataRow>();
            foreach (DataRow r in selTbl.Rows)
            {
                String stringKey = ""; 
                foreach (String kf in keyFields)
                {
                    object keyValue = r[kf];
                    stringKey = stringKey + (keyValue == null ? "null" : keyValue.ToString()) + "_";
                }
                existingValues.Add(stringKey, r);
            }

            //create tables
            DataTable insTbl = new DataTable();
            DataTable updTbl = new DataTable();
            DataTable delTbl = new DataTable();
            for (int i = 1; i < insCmd.Parameters.Count; i++)
            {
                SqlParameter p = insCmd.Parameters[i];
                String pName = p.ParameterName.Substring(1);
                Type t = SqlDbTypeToType(p.SqlDbType);
                insTbl.Columns.Add(pName, t);
                updTbl.Columns.Add(pName, t);
                delTbl.Columns.Add(pName, t);
            }

            //update or insert
            foreach (var childRow in section.Entities)
            {
                DataRow r = FillDataRow(insTbl, childRow);
                r["Ref"] = entityId;

                String stringKey = "";
                foreach (String kf in keyFields)
                {
                    object keyValue = r[kf];
                    stringKey = stringKey + (keyValue == null ? "null" : keyValue.ToString()) + "_";
                }

                if (existingValues.ContainsKey(stringKey))
                {
                    //compare
                    DataRow eRow = existingValues[stringKey];
                    if (eRow != null)
                    {
                        if (!eRow.ItemArray.SequenceEqual(r.ItemArray))
                        {
                            AddRow(updTbl, r);
                        }
                        existingValues[stringKey] = null; //not to be deleted
                    }
                }
                else
                    AddRow(insTbl, r);
            }

            //remove
            foreach (KeyValuePair<String, DataRow> kvp in existingValues)
            {
                if (kvp.Value != null)
                {
                    AddRow(delTbl, kvp.Value);
                }
            }

            //delete
            if (delTbl.Rows.Count > 0)
            {
                delCmdBatch.Parameters["@Data"].Value = delTbl;
                delCmdBatch.ExecuteNonQuery();
            }

            //update
            if (updTbl.Rows.Count > 0)
            {
                updCmdBatch.Parameters["@Data"].Value = updTbl;
                updCmdBatch.ExecuteNonQuery();
            }

            //insert
            if (insTbl.Rows.Count > 0)
            {
                insCmdBatch.Parameters["@Data"].Value = insTbl;
                insCmdBatch.ExecuteNonQuery();
            }
        }
        */

        private void UpdateTabularSectionBatchAll(Guid entityId, TabularSection section, SqlCommand insCmd, SqlCommand updCmdBatch)
        {
            String[] keyFields = section.KeyFields;

            //create table
            DataTable updTbl = new DataTable();
            for (int i = 1; i < insCmd.Parameters.Count; i++)
            {
                SqlParameter p = insCmd.Parameters[i];
                String pName = p.ParameterName.Substring(1);
                Type t = SqlDbTypeToType(p.SqlDbType);
                if (!pName.ToLower().Equals("ref"))
                    updTbl.Columns.Add(pName, t);
            }

            FillDataTable(updTbl, section, false);

            updCmdBatch.Parameters["@Ref"].Value = entityId;
            updCmdBatch.Parameters["@Data"].Value = updTbl;
            updCmdBatch.ExecuteNonQuery();
        }

        private void AddRow(DataTable tbl, DataRow row)
        {
            DataRow newRow = tbl.NewRow();
            newRow.ItemArray = row.ItemArray;
            tbl.Rows.Add(newRow);
        }

        private SqlCommand FillParameters(SqlCommand cmd, Entity row)
        {
            for (int i = 1; i < cmd.Parameters.Count; i++)
            {
                SqlParameter param = cmd.Parameters[i];
                String attrName = param.ParameterName.Substring(1);
                if (row.Attributes.ContainsKey(attrName))
                {
                    String value = row.Attributes[attrName];
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

        private DataRow FillDataRow(DataTable tbl, Entity row, bool includeRef)
        {
            DataRow datarow = tbl.NewRow();
            for (int i = 0; i < tbl.Columns.Count; i++)
            {
                DataColumn param = tbl.Columns[i];
                String attrName = param.ColumnName;
                if (row.Attributes.ContainsKey(attrName) && !(attrName.ToLower().Equals("ref") && !includeRef))
                {
                    String value = row.Attributes[attrName];
                    if (value.ToLower().Equals("null"))
                        datarow[i] = DBNull.Value;
                    else
                    {
                        if (param.DataType == typeof(Guid))
                        {
                            if (value.Equals("00000000-0000-0000-0000-000000000000"))
                                datarow[i] = DBNull.Value;
                            else
                                datarow[i] = new Guid(value);
                        }
                        else
                            datarow[i] = value;
                    }
                }
            }

            return datarow;
        }

        private void FillDataTable(DataTable tbl, TabularSection section, bool includeRow, bool checkDuplicates = true)
        {
            Dictionary<String, object> keyValues = null;

            String[] keyFields = section.KeyFields;
            if (keyFields != null)
                if (keyFields.Length > 0 && checkDuplicates)
                    keyValues = new Dictionary<string, object>(); //will check duplicates

            foreach (var childRow in section.Entities)
            {
                DataRow r = FillDataRow(tbl, childRow, includeRow);

                if (keyValues != null) //check duplicates
                {
                    String stringKey = "";
                    foreach (String kf in keyFields)
                    {
                        object keyValue = r[kf];
                        stringKey = stringKey + (keyValue == null ? "null" : keyValue.ToString().ToLower()) + "_";
                    }
                    if (!keyValues.ContainsKey(stringKey)) //ignore duplicates
                        keyValues.Add(stringKey, null);
                    else
                        continue;
                }

                tbl.Rows.Add(r);
            }
        }

        private Type SqlDbTypeToType(SqlDbType dbType)
        {
            switch (dbType)
            {
                case SqlDbType.BigInt:
                    return typeof(Int64);
                case SqlDbType.Bit:
                    return typeof(bool);
                case SqlDbType.Char:
                    return typeof(char);
                case SqlDbType.Date:
                    return typeof(DateTime);
                case SqlDbType.DateTime:
                    return typeof(DateTime);
                case SqlDbType.DateTime2:
                    return typeof(DateTime);
                case SqlDbType.Decimal:
                    return typeof(Decimal);
                case SqlDbType.Float:
                    return typeof(float);
                case SqlDbType.Int:
                    return typeof(int);
                case SqlDbType.NChar:
                    return typeof(char);
                case SqlDbType.NText:
                    return typeof(String);
                case SqlDbType.NVarChar:
                    return typeof(String);
                case SqlDbType.Real:
                    return typeof(decimal);
                case SqlDbType.SmallDateTime:
                    return typeof(DateTime);
                case SqlDbType.SmallInt:
                    return typeof(Int16);
                case SqlDbType.Text:
                    return typeof(String);
                case SqlDbType.TinyInt:
                    return typeof(Int16);
                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);
                case SqlDbType.VarChar:
                    return typeof(String);
                case SqlDbType.Xml:
                    return typeof(String);
                default:
                    throw new System.Exception("Unsupported type:" + dbType.ToString());
            }
        }

    }
}
