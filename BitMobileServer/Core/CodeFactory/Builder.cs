using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using CodeFactory.CodeGeneration.Templates.Database;
using CodeFactory.CodeGeneration.Templates.Code;
using CodeFactory.CodeGeneration.Templates.Sync;
using CodeFactory.CodeGeneration.Templates.AdminApp;

using Common;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Data.SqlClient;
using System.Xml;
using CodeFactory.DatabaseFactory;

namespace CodeFactory
{
    public class Builder
    {
        private bool deleteTempFiles = true;

        public void Build(Solution solution, bool onlyResources, bool checkTabularSectionKey = false, bool filtersOnly = false)
        {
            String configurationFile = System.IO.Path.Combine(solution.SolutionFolder, @"configuration\configuration.xml");

            Factory factory = new Factory();

            Dictionary<String, Parameter> globalParameters = factory.GetGlobalParameters(configurationFile);
            List<Entity> entities = new List<Entity>();
            entities.AddRange(factory.GetEntities(configurationFile, checkTabularSectionKey));
            entities.AddRange(factory.GetResources(solution.DeviceResourceFolder));
            entities.AddRange(factory.GetAdmin());
            Dictionary<String, List<Constant>> constants = factory.GetConstants(configurationFile);

            ConfigVersion configVersion = factory.GetVersion(configurationFile);
            Config config = new Config(configVersion, solution.DatabaseName, entities, globalParameters, constants);

            if (onlyResources)
                BuildResources(config, solution);
            else
            {
                if (filtersOnly)
                    BuildFilters(config, solution);
                else
                {
                    BuildAll(config, solution);
                    InsertConstants(solution);
                }
            }
        }

        private Dictionary<String, String> GetTempFiles(String solutionFolder)
        {
            Dictionary<String, String> tempFiles = new Dictionary<string, string>();
            tempFiles.Add("dropdatabase", System.IO.Path.Combine(solutionFolder, @"Database\dropDatabase.sql"));
            tempFiles.Add("createdatabase", System.IO.Path.Combine(solutionFolder, @"Database\createDatabase.sql"));
            tempFiles.Add("database", System.IO.Path.Combine(solutionFolder, @"Database\database.sql"));
            tempFiles.Add("syncconfig", System.IO.Path.Combine(solutionFolder, @"configuration\syncconfig.xml"));
            tempFiles.Add("filters", System.IO.Path.Combine(solutionFolder, @"Database\filters.sql"));
            tempFiles.Add("syncpatch", System.IO.Path.Combine(solutionFolder, @"Database\syncpatch.sql"));
            tempFiles.Add("admin", System.IO.Path.Combine(solutionFolder, @"Database\admin.sql"));
            tempFiles.Add("keyspatch", System.IO.Path.Combine(solutionFolder, @"Database\keyspatch.sql"));
            tempFiles.Add("azurekeyspatch", System.IO.Path.Combine(solutionFolder, @"Database\azurekeyspatch.sql"));
            tempFiles.Add("dataload", System.IO.Path.Combine(solutionFolder, @"Database\dataload.sql"));
            tempFiles.Add("sqlite", System.IO.Path.Combine(solutionFolder, @"Database\sqlite.sql"));
            return tempFiles;
        }

        private void BuildFilters(Config config, Solution solution)
        {
            //remove existing filters
            try
            {
                using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("[admin].RemoveFilters", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
            }

            //create new ones
            String solutionFolder = solution.SolutionFolder;

            if (deleteTempFiles)
            {
                solutionFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(solutionFolder);
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"Database"));
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"configuration"));
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"sqlite"));
            }

            solution.Log("Start updating solution");

            Dictionary<String, String> tempFiles = GetTempFiles(solutionFolder);

            DatabaseFactory.DatabaseFactory dbf = new DatabaseFactory.DatabaseFactory(solution.ConnectionString);

            //filters
            String filtersScript;
            if (solution.IsAsured)
                filtersScript = new FiltersAzure(config).TransformText();
            else
                filtersScript = new Filters(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["filters"], filtersScript);
            dbf.RunScript(filtersScript);

            solution.Log("Filters ok");

            //sync patch
            String syncPatchScript = new SyncPatch2(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["syncpatch"], syncPatchScript);
            dbf.RunScript(syncPatchScript);

            solution.Log("Sync patch ok");

            solution.Log("Solution has been successfully updated");
        }

        private void BuildAll(Config config, Solution solution)
        {
            String solutionFolder = solution.SolutionFolder;
            CreateSolutionFolders(solutionFolder);

            if (deleteTempFiles)
            {
                solutionFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(solutionFolder);
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"Database"));
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"configuration"));
                Directory.CreateDirectory(System.IO.Path.Combine(solutionFolder, @"sqlite"));
            }

            solution.Log("Start building solution");

            Dictionary<String, String> tempFiles = GetTempFiles(solutionFolder);

            DatabaseFactory.DatabaseFactory dbf = null;

            //init database
            dbf = new DatabaseFactory.DatabaseFactory(solution.DatabaseServer);

            String dropDatabaseScript;
            if (solution.IsAsured)
                dropDatabaseScript = new DropDatabaseAzure(config).TransformText();
            else
                dropDatabaseScript = new DropDatabase(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["dropdatabase"], dropDatabaseScript);
            try
            {
                dbf.RunScript(dropDatabaseScript);
            }
            catch (Exception)
            {
            }

            String createDatabaseScript;
            if(solution.IsAsured)
                createDatabaseScript = new CreateDatabaseAzure(config).TransformText();
            else
                createDatabaseScript = new CreateDatabase(config).TransformText();

            System.IO.File.WriteAllText(tempFiles["createdatabase"], createDatabaseScript);
            dbf.RunScript(createDatabaseScript);

            solution.Log("DB scripts ok");

            //create database
            dbf = new DatabaseFactory.DatabaseFactory(solution.ConnectionString);
            String dbScript = new Database(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["database"], dbScript);
            dbf.RunScript(dbScript);

            solution.Log("Database ok");

            System.Data.SqlClient.SqlConnection.ClearAllPools();

            //sort tables
            SyncOrder.Initialize(solution.ConnectionString);
            SyncOrder.Sort(config.Entities);
            
            //provisioning
            String syncConfig = tempFiles["syncconfig"];
            System.IO.File.WriteAllText(syncConfig, new SyncConfig(config, solution.ConnectionString).TransformText());
            dbf.Provision(new String[] { "/mode:provision", String.Format("/scopeconfig:{0}", syncConfig) });

            solution.Log("Provision ok");

            //filters
            String filtersScript;
            if (solution.IsAsured)
                filtersScript = new FiltersAzure(config).TransformText();
            else
                filtersScript = new Filters(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["filters"], filtersScript);
            dbf.RunScript(filtersScript);

            solution.Log("Filters ok");

            //sync patch
            String syncPatchScript = new SyncPatch2(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["syncpatch"], syncPatchScript);
            dbf.RunScript(syncPatchScript);

            solution.Log("Sync patch ok");

            //admin
            dbScript = new DatabaseAdmin(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["admin"], dbScript);
            dbf.RunScript(dbScript);

            solution.Log("Admin ok");

            //clustered primary keys patch
            config.BuildClusteredPrimaryKeys(solution.ConnectionString);
            String keysPatchScript = new KeysPatch(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["keyspatch"], keysPatchScript);
            dbf.RunScript(keysPatchScript);

            solution.Log("Clustered keys patch ok");

            if (solution.IsAsured)
            {
                //clustered primary keys patch
                config.BuildNonClusteredPrimaryKeys(solution.ConnectionString);
                String azureKeysPatchScript = new AzureKeysPatch(config).TransformText();
                System.IO.File.WriteAllText(tempFiles["azurekeyspatch"], azureKeysPatchScript);
                dbf.RunScript(azureKeysPatchScript);

                solution.Log("Non clustered keys patch ok");
                solution.Log("Default NEWID values on tabular sections ok");
            }
            else
            {
                String nonAzureKeysPatchScript = new NonAzureKeysPatch(config).TransformText();
                System.IO.File.WriteAllText(tempFiles["azurekeyspatch"], nonAzureKeysPatchScript);
                dbf.RunScript(nonAzureKeysPatchScript);
                solution.Log("Default NEWSEQUENTIALID values on tabular sections ok");
            }
    
            //initial load
            String dataLoadScript = new DataLoad(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["dataload"], dataLoadScript);
            dbf.RunScript(dataLoadScript);
            
            solution.Log("initial data load ok");

            //sqlite
            String sqlieScript = new SQLiteDatabase(config).TransformText();
            System.IO.File.WriteAllText(tempFiles["sqlite"], sqlieScript);
            if(!System.IO.Directory.Exists(System.IO.Path.Combine(solution.SolutionFolder,"sqlite")))
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(solution.SolutionFolder,"sqlite"));
            String sqlitedb = System.IO.Path.Combine(solution.SolutionFolder, @"sqlite\sqlite.db");
            if (System.IO.File.Exists(sqlitedb))
                System.IO.File.Delete(sqlitedb);

            //SQLiteDatabaseFactory dbflite = new SQLiteDatabaseFactory(sqlitedb);
            //dbflite.CreateDatabase();
            //dbflite.RunScript(sqlieScript);
            //dbflite.InsertMetadata(solution.ConfigurationFile);

            solution.Log("sqlite ok");

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\Server.cs"), new Server(config).TransformText());

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\Client.cs"), new Client(config).TransformText());

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\ClientMetadata.cs"), new ClientMetadata(config).TransformText());

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\ClientConstants.cs"), new ClientConstants(config).TransformText());

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\Fake.cs"), new Fake().TransformText());

            System.IO.File.WriteAllText(System.IO.Path.Combine(solution.SolutionFolder, @"Code\DbFake.cs"), new DbFake().TransformText());

            solution.Log("Code generation ok");

            BuildClientDll(solution.SolutionFolder);
            solution.Log("Client dll ok");

            BuildServerDll(solution.SolutionFolder);
            solution.Log("Server dll ok");

            BuildResources(config, solution);
            solution.Log("Resources ok");

            if (deleteTempFiles)
            {
                foreach (var entry in tempFiles)
                    System.IO.File.Delete(entry.Value);
            }

            solution.Log("Solution has been successfully built");
        }

        private void InsertConstants(Solution solution)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(solution.ConfigurationFile);

            SqlConnection conn = new SqlConnection(solution.ConnectionString);
            conn.Open();
            using (conn)
            {
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    XmlNodeList entityNodes = doc.DocumentElement.SelectNodes("//Configuration/Constants/Entity");
                    foreach (XmlNode entityNode in entityNodes)
                    {
                        String entityName = entityNode.Attributes["Name"].Value;
                        String[] arr = entityName.Split('.');
                        //entityName = String.Format("[{0}].[{1}]", arr[0], arr[1]);

                        SqlCommand cmd = new SqlCommand();
                        cmd = new SqlCommand(String.Format("[{0}].[{1}_adm_insert]", arr[0], arr[1]), conn, tran);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(cmd);

                        System.Xml.XmlNodeList rows = entityNode.SelectNodes("Row");
                        foreach (System.Xml.XmlNode row in rows)
                        {
                            FillParameters(cmd, row);
                            cmd.ExecuteNonQuery();
                        }
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

        private SqlCommand FillParameters(SqlCommand cmd, XmlNode row, bool markdeleted = false)
        {
            for (int i = 1; i < cmd.Parameters.Count; i++)
            {
                SqlParameter param = cmd.Parameters[i];
                XmlAttribute a = row.Attributes[param.ParameterName.Substring(1)];
                if (a != null)
                {
                    String value = a.Value;
                    if (param.DbType == System.Data.DbType.Guid)
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

            return cmd;
        }

        public void CreateSolutionFolders(String path)
        {
            String[] arr = { "bin", "code", "configuration", "database", "resource", "log", "access", "sqlite" };

            if(!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            foreach (String s in arr)
            {
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(path, s)))
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, s));
            }

            if (!System.IO.File.Exists(String.Format(@"{0}\access\licenses.txt", path)))
                System.IO.File.WriteAllText(String.Format(@"{0}\access\licenses.txt", path), "0");
        }

        private void BuildResources(Config config, Solution solution)
        {
            if (solution.ResourcesExists)
            {
                //check if settings.xml exists
                if (!System.IO.File.Exists(solution.SettingsFile))
                    throw new Exception("File settings.xml not found in resource folder");

                CheckMetaVersion(config, solution);

                PopulateResources(solution.ConnectionString, solution.DeviceResourceFolder);
            }
        }

        private void CheckMetaVersion(Config config, Solution solution)
        {
            Common.Logon.CheckSupportedMetaVersion(solution.Name, config.ConfigVersion.Version);
        }

        private void PopulateResources(String connectionString, String directory)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString);
            conn.Open();
            using (conn)
            {
                String schema = "resource";// new System.IO.DirectoryInfo(directory).Name;
                foreach (String app in System.IO.Directory.GetDirectories(directory))
                {
                    String appName = new System.IO.FileInfo(app).Name;

                    foreach (String dir in System.IO.Directory.GetDirectories(app))
                    {
                        String dirName = new System.IO.FileInfo(dir).Name;

                        System.Data.SqlClient.SqlCommand deleteCmd = new System.Data.SqlClient.SqlCommand();
                        deleteCmd.Connection = conn;
                        deleteCmd.CommandTimeout = 300;
                        deleteCmd.CommandText = String.Format("DELETE FROM {0}.{1} WHERE Name LIKE '{2}/%'", schema, dirName, appName);
                        deleteCmd.ExecuteNonQuery();

                        SaveDirectoryToDB(conn, schema, appName, dirName, dir);
                    }
                }
            }
        }

        private static void SaveDirectoryToDB(SqlConnection conn, String schema, String appName, String baseDirectory, String directory)
        {
            foreach (String fileName in System.IO.Directory.GetFiles(directory))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandTimeout = 300;
                cmd.Connection = conn;
                cmd.CommandText =
                    String.Format("INSERT INTO {0}.{1}([Id],[Name],[Data],[Parent]) VALUES(@Id, @Name, @Data, @Parent)"
                    , schema, baseDirectory);

                System.Data.SqlClient.SqlParameter p;

                p = cmd.Parameters.Add("@Id", System.Data.SqlDbType.UniqueIdentifier);
                p.Value = System.Guid.NewGuid();

                p = cmd.Parameters.Add("@Name", System.Data.SqlDbType.VarChar);
                p.Value = String.Format("{0}/{1}", appName, new System.IO.FileInfo(fileName).Name);

                p = cmd.Parameters.Add("@Data", System.Data.SqlDbType.Text);
                p.Value = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));

                p = cmd.Parameters.Add("@Parent", System.Data.SqlDbType.VarChar);

                string[] directories = Path.GetDirectoryName(fileName).Split(Path.DirectorySeparatorChar);

                List<string> result = new List<string>();
                for (int i = directories.Length - 1; i >= 0; i--)
                {
                    if (directories[i] == baseDirectory)
                        break;
                    else
                        result.Add(directories[i]);
                }
                result.Reverse();

                string resultPath = "";
                foreach (var item in result)
                    resultPath += Path.DirectorySeparatorChar + item;

                p.Value = resultPath;
                cmd.ExecuteNonQuery();
            }

            foreach (var subDir in Directory.GetDirectories(directory))
                SaveDirectoryToDB(conn, schema, appName, baseDirectory, subDir);
        }

        ////////////////////////////////////////////////////////DLLs///////////////////////////////////////////////////////////////

        private void CheckCompilerResults(System.CodeDom.Compiler.CompilerResults results)
        {
            if (results.Errors.Count > 0)
            {
                String error = "";
                foreach (System.CodeDom.Compiler.CompilerError CompErr in results.Errors)
                {
                    error = error +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
                throw new Exception(error);
            }
        }

        private bool BuildFakeSyncLibrary(string rootPath)
        {
            String codeFile = System.IO.Path.Combine(rootPath, @"code\Fake.cs");
            String fileName = System.IO.Path.Combine(rootPath, @"bin\SyncLibrary.dll");

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = fileName;
            parameters.TempFiles = new TempFileCollection(String.Format(@"{0}\bin", rootPath));

            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(@"System.Linq.dll");

            System.CodeDom.Compiler.CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, codeFile);
            CheckCompilerResults(results);

            return true;
        }

        private bool BuildDbFakeLibrary(string rootPath)
        {
            String codeFile = System.IO.Path.Combine(rootPath, @"code\DbFake.cs");
            String fileName = System.IO.Path.Combine(rootPath, @"bin\DbEngine.dll");

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = fileName;
            parameters.TempFiles = new TempFileCollection(String.Format(@"{0}\bin", rootPath));

            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(@"System.Linq.dll");

            System.CodeDom.Compiler.CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, codeFile);
            CheckCompilerResults(results);

            return true;
        }

        private void BuildClientDll(string rootPath)
        {
            BuildDbFakeLibrary(rootPath);
            BuildFakeSyncLibrary(rootPath);

            String clientFileName = System.IO.Path.Combine(rootPath, @"code\Client.cs");
            String clientMetadataFileName = System.IO.Path.Combine(rootPath, @"code\ClientMetadata.cs");
            String clientConstantsFileName = System.IO.Path.Combine(rootPath, @"code\ClientConstants.cs");

            String fileName = System.IO.Path.Combine(rootPath, @"bin\Client.dll");
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = fileName;
            parameters.TempFiles = new TempFileCollection(String.Format(@"{0}\bin", rootPath));

            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(System.IO.Path.Combine(rootPath, @"bin\SyncLibrary.dll"));
            parameters.ReferencedAssemblies.Add(System.IO.Path.Combine(rootPath, @"bin\DbEngine.dll"));

            string[] fileArray = new string[] { clientFileName, clientMetadataFileName, clientConstantsFileName };
            System.CodeDom.Compiler.CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, fileArray);
            CheckCompilerResults(results);
        }

        private void BuildServerDll(String solutionFolder)
        {
            String codeFile = System.IO.Path.Combine(solutionFolder, @"code\Server.cs");
            AppDomain domain = AppDomain.CurrentDomain;
            String fileName = String.Format(@"{0}\bin\Server{1:yyyyMMddHHmmss}.dll", solutionFolder, new System.IO.FileInfo(codeFile).LastWriteTime);

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = fileName;
            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.TempFiles = new TempFileCollection(String.Format(@"{0}\bin", solutionFolder));

            parameters.ReferencedAssemblies.Add(System.IO.Path.Combine(domain.BaseDirectory, @"bin\SyncLibrary.dll"));
            parameters.ReferencedAssemblies.Add(System.IO.Path.Combine(domain.BaseDirectory, @"bin\Common.dll"));

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, System.IO.File.ReadAllText(codeFile));
            CheckCompilerResults(results);
        }

    }

}
