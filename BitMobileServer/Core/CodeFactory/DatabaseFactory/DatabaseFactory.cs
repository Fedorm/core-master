using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Data.SqlClient;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data;

using Microsoft.Synchronization.ClientServices.Configuration;
using Microsoft.Synchronization.ClientServices;
using System.Configuration;


namespace CodeFactory.DatabaseFactory
{
    public class DatabaseFactory
    {
        private String connectionString;

        public DatabaseFactory(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public void RunScript(String script, bool inTransaction = false)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            using (conn)
            {
                SqlTransaction tran = null;
                if(inTransaction)
                    tran = conn.BeginTransaction();
                try
                {
                    String[] commands = script.Split(new String[] { "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String command in commands)
                    {
                        String s = command;//.Replace("\r\n", "");
                        if(!String.IsNullOrEmpty(s))
                        {
                            SqlCommand cmd = new SqlCommand(s, conn, tran);
                            cmd.CommandTimeout = 60;
                            cmd.ExecuteNonQuery();
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

        public void Provision(String[] args)
        {
            ArgsParser parser = ArgsParser.ParseArgs(args);
            System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = parser.ConfigFile }, ConfigurationUserLevel.None);
            SyncConfigurationSection syncConfig = config.GetSection(Constants.SyncConfigurationSectionName) as SyncConfigurationSection;

            SelectedConfigSections selectedConfig = FillDefaults(parser, syncConfig);
            DbSyncScopeDescription scopeDescription = GetDbSyncScopeDescription(selectedConfig);

            try
            {
                SqlSyncScopeProvisioning prov = new SqlSyncScopeProvisioning(new SqlConnection(selectedConfig.SelectedTargetDatabase.GetConnectionString()),
                                    scopeDescription, selectedConfig.SelectedSyncScope.IsTemplateScope ? SqlSyncScopeProvisioningType.Template : SqlSyncScopeProvisioningType.Scope);
                prov.CommandTimeout = 60;

                // Note: Deprovisioning does not work because of a bug in the provider when you set the ObjectSchema property to “dbo”. 
                // The workaround is to not set the property (it internally assumes dbo in this case) so that things work on deprovisioning.
                if (!String.IsNullOrEmpty(selectedConfig.SelectedSyncScope.SchemaName))
                {
                    prov.ObjectSchema = selectedConfig.SelectedSyncScope.SchemaName;
                }

                foreach (SyncTableConfigElement tableElement in selectedConfig.SelectedSyncScope.SyncTables)
                {
                    // Check and set the SchemaName for individual table if specified
                    if (!string.IsNullOrEmpty(tableElement.SchemaName))
                    {
                        prov.Tables[tableElement.GlobalName].ObjectSchema = tableElement.SchemaName;
                    }

                    prov.Tables[tableElement.GlobalName].FilterClause = tableElement.FilterClause;
                    foreach (FilterColumnConfigElement filterCol in tableElement.FilterColumns)
                    {
                        prov.Tables[tableElement.GlobalName].FilterColumns.Add(scopeDescription.Tables[tableElement.GlobalName].Columns[filterCol.Name]);
                    }
                    foreach (FilterParameterConfigElement filterParam in tableElement.FilterParameters)
                    {
                        CheckFilterParamTypeAndSize(filterParam);
                        prov.Tables[tableElement.GlobalName].FilterParameters.Add(new SqlParameter(filterParam.Name, (SqlDbType)Enum.Parse(typeof(SqlDbType), filterParam.SqlType, true)));
                        prov.Tables[tableElement.GlobalName].FilterParameters[filterParam.Name].Size = filterParam.DataSize;
                    }
                }

                // enable bulk procedures.
                prov.SetUseBulkProceduresDefault(selectedConfig.SelectedSyncScope.EnableBulkApplyProcedures);

                // Create a new set of enumeration stored procs per scope. 
                // Without this multiple scopes share the same stored procedure which is not desirable.
                prov.SetCreateProceduresForAdditionalScopeDefault(DbSyncCreationOption.Create);

                if (selectedConfig.SelectedSyncScope.IsTemplateScope)
                {
                    if (!prov.TemplateExists(selectedConfig.SelectedSyncScope.Name))
                    {
                        //Log("Provisioning Database {0} for template scope {1}...", selectedConfig.SelectedTargetDatabase.Name, selectedConfig.SelectedSyncScope.Name);
                        prov.Apply();
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Database {0} already contains a template scope {1}. Please deprovision the scope and retry.", selectedConfig.SelectedTargetDatabase.Name,
                            selectedConfig.SelectedSyncScope.Name));
                    }
                }
                else
                {
                    if (!prov.ScopeExists(selectedConfig.SelectedSyncScope.Name))
                    {
                        //Log("Provisioning Database {0} for scope {1}...", selectedConfig.SelectedTargetDatabase.Name, selectedConfig.SelectedSyncScope.Name);
                        prov.Apply();
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Database {0} already contains a scope {1}. Please deprovision the scope and retry.", selectedConfig.SelectedTargetDatabase.Name,
                            selectedConfig.SelectedSyncScope.Name));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        private static DbSyncScopeDescription GetDbSyncScopeDescription(SelectedConfigSections selectedConfig)
        {
            DbSyncScopeDescription desc = new DbSyncScopeDescription(selectedConfig.SelectedSyncScope.Name);

            using (SqlConnection conn = new SqlConnection(selectedConfig.SelectedTargetDatabase.GetConnectionString()))
            {
                conn.Open();
                foreach (SyncTableConfigElement table in selectedConfig.SelectedSyncScope.SyncTables)
                {
                    DbSyncTableDescription tableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable(table.Name, conn);

                    // Ensure all specified columns do belong to the table on the server.
                    foreach (SyncColumnConfigElement colElem in table.SyncColumns)
                    {
                        if (tableDesc.Columns.Where((e) => e.UnquotedName.Equals(colElem.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() == null)
                        {
                            throw new InvalidOperationException(string.Format("Table '{0}' does not have a column '{1}' defined in the target database. Please check your SyncColumn definitions",
                                table.Name, colElem.Name));
                        }
                    }

                    List<DbSyncColumnDescription> columnsToRemove = new List<DbSyncColumnDescription>();

                    // Mark timestamp columns for removal
                    columnsToRemove.AddRange(tableDesc.Columns.Where(
                        e => ((SqlDbType)Enum.Parse(typeof(SqlDbType), e.Type, true)) == SqlDbType.Timestamp)
                        );

                    if (!table.IncludeAllColumns || table.SyncColumns.Count > 0)
                    {
                        //Users wants a subset of columns. Remove the ones they are not interested in
                        foreach (DbSyncColumnDescription columnDesc in tableDesc.Columns)
                        {
                            SyncColumnConfigElement configElement = table.SyncColumns.Cast<SyncColumnConfigElement>().FirstOrDefault((e) => e.Name.Equals(columnDesc.UnquotedName, StringComparison.InvariantCultureIgnoreCase));
                            if (configElement == null)
                            {
                                // Found a column that was not specified by the user. Remove it
                                columnsToRemove.Add(columnDesc);
                            }
                            else
                            {
                                columnDesc.IsNullable = configElement.IsNullable;
                                columnDesc.IsPrimaryKey = configElement.IsPrimaryKey;
                            }
                        }
                    }

                    // Remove columns marked for removal
                    columnsToRemove.ForEach((e) => tableDesc.Columns.Remove(e));

                    // Check to see that columns count is greater than 0
                    if (tableDesc.Columns.Count == 0)
                    {
                        throw new InvalidOperationException(
                            string.Format("SyncTable '{0}' has zero SyncColumns configured for sync. Either set IncludeAllColumns to true or specify atleast one SyncColumn.", table.Name));
                    }

                    // Fill in global name
                    if (!string.IsNullOrEmpty(table.GlobalName))
                    {
                        tableDesc.GlobalName = table.GlobalName;
                    }

                    desc.Tables.Add(tableDesc);
                }
            }

            return desc;
        }

        private static SelectedConfigSections FillDefaults(ArgsParser parser, SyncConfigurationSection syncConfig)
        {
            SelectedConfigSections sections = new SelectedConfigSections();

            if (string.IsNullOrEmpty(parser.ScopeName) && syncConfig.SyncScopes.Count == 1)
            {
                sections.SelectedSyncScope = syncConfig.SyncScopes.Cast<SyncScopeConfigElement>().First();
                parser.ScopeName = sections.SelectedSyncScope.Name;
            }
            else
            {
                sections.SelectedSyncScope = syncConfig.SyncScopes.Cast<SyncScopeConfigElement>().Single((e) => e.Name.Equals(parser.ScopeName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (string.IsNullOrEmpty(parser.TargetDatabaseName) && syncConfig.Databases.Count == 1)
            {
                sections.SelectedTargetDatabase = syncConfig.Databases.Cast<TargetDatabaseConfigElement>().First();
                parser.TargetDatabaseName = sections.SelectedTargetDatabase.Name;
            }
            else
            {
                sections.SelectedTargetDatabase = syncConfig.Databases.Cast<TargetDatabaseConfigElement>().Single((e) => e.Name.Equals(parser.TargetDatabaseName, StringComparison.InvariantCultureIgnoreCase));
            }
            
            return sections;
        }

        private static void CheckFilterParamTypeAndSize(FilterParameterConfigElement filterParam)
        {
            if (filterParam.SqlType == "VARCHAR(100)")
            {
                filterParam.SqlType = "varchar";
                filterParam.DataSize = 100;
            }

            // Check that all relavant properties for a SqlParameter are present.

            // Check that name starts with a @
            if (!filterParam.Name.Trim().StartsWith("@", StringComparison.Ordinal))
            {
                throw new Exception(string.Format("FilterParameter '{0}' Name property does not start with '@'.", filterParam.Name));
            }

            // First check that SqlType is a valid SqlDbType enum
            if (Enum.GetNames(typeof(SqlDbType)).Where((e) => e.Equals(filterParam.SqlType, StringComparison.OrdinalIgnoreCase)).Count() == 0)
            {
                throw new Exception(string.Format("SqlType '{0}' for filter parameter {1} is not a valid SqlDbType enum.", filterParam.SqlType, filterParam.Name));
            }

            // Ensure that the DataSize attribute has a value set for the required types
            SqlDbType type = (SqlDbType)Enum.Parse(typeof(SqlDbType), filterParam.SqlType, true);
            switch (type)
            {
                case SqlDbType.Binary:
                case SqlDbType.Char:
                case SqlDbType.Image:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    if (filterParam.DataSize <= 0)
                    {
                        throw new Exception(string.Format("Filter parameter '{0}' must specify a non zero number for the DataSize attribute.", filterParam.Name));
                    }
                    break;
            }
        }

    }
    
    class SelectedConfigSections
    {
        public SyncScopeConfigElement SelectedSyncScope;
        public TargetDatabaseConfigElement SelectedTargetDatabase;
    }
    
}
