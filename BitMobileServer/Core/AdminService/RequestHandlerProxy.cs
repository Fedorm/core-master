using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Collections.Generic;

using System.ServiceModel;
using System.ServiceModel.Activation;
using FileHelperForCloud;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Net;
using System.Threading;
using System.IO.Compression;

namespace AdminService
{
    public class RequestHandlerProxy : MarshalByRefObject
    {
        public RequestHandlerProxy()
        {          
        }

        public Stream UploadMetadata(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UploadMetadataInternal(solution, messageBody, false);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadMetadata2(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UploadMetadataInternal(solution, messageBody, true);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadMetadataFiltersOnly(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UploadMetadataInternal(solution, messageBody, true, true);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadMetadataAsync(Common.Solution solution, Stream messageBody)
        {
            try
            {
                System.IO.MemoryStream ms = new MemoryStream();
                messageBody.CopyTo(ms);
                ms.Position = 0;

                AsyncDeployMetadataThread t = new AsyncDeployMetadataThread(this, solution, ms, false);
                t.Start();
                return Common.Utils.MakeTextAnswer(String.Format("accepted"));
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadMetadata2Async(Common.Solution solution, Stream messageBody)
        {
            try
            {
                System.IO.MemoryStream ms = new MemoryStream();
                messageBody.CopyTo(ms);
                ms.Position = 0;

                AsyncDeployMetadataThread t = new AsyncDeployMetadataThread(this, solution, ms, true);
                t.Start();
                return Common.Utils.MakeTextAnswer(String.Format("accepted"));
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }
        public void UploadMetadataInternal(Common.Solution solution, Stream messageBody, bool checkTabularSectionKey = false, bool filtersOnly = false)
        {
            Common.Solution.ChangeHostState(solution.Name, "stop");

            System.IO.File.WriteAllText(solution.ConfigurationFile, new System.IO.StreamReader(messageBody).ReadToEnd());
            new CodeFactory.Builder().Build(solution, false, checkTabularSectionKey, filtersOnly);

            ArchiveFileSystemInternal(solution);

            Common.Solution.ChangeHostState(solution.Name, "start");
        }

        public Stream ReDeployMetadata(Common.Solution solution)
        {
            String scope = solution.Name;
            try
            {
                String path = Common.Solution.GetSolutionFolder(scope);

                if (!System.IO.Directory.Exists(path))
                    throw new Exception(String.Format("Solution '{0}' does not exist", scope));

                Common.Solution.ChangeHostState(scope, "stop");
                new CodeFactory.Builder().Build(Common.Solution.CreateFromContext(scope), false);

                ArchiveFileSystemInternal(solution);

                Common.Solution.ChangeHostState(scope, "start");

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, scope);
            }
        }

        public Stream DeploySolutionPackage(Common.Solution solution, Stream messageBody)
        {
            String scope = solution.Name;
            try
            {
                //create temp dir
                String tempDir = Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(tempDir);

                //save stream to file
                String packagePath = System.IO.Path.Combine(tempDir, "package.zip");
                using (FileStream tempZipFileStream = new FileStream(packagePath, FileMode.Create))
                {
                    messageBody.CopyTo(tempZipFileStream);
                }

                //unzip folder
                FileHelper.CreateUnzipFolder(packagePath, tempDir, "package");

                //deploy metadata
                String metadatafile = System.IO.Path.Combine(tempDir, "package", "metadata.xml");
                using (System.IO.FileStream fs = new FileStream(metadatafile, FileMode.Open))
                {
                    UploadMetadataInternal(solution, fs);
                }

                //deploy data
                String datafile = System.IO.Path.Combine(tempDir, "package", "data.xml");
                UploadDataInternal(solution, null, false, false, null, datafile);

                //deploy resources
                String resource = System.IO.Path.Combine(tempDir, "package", "resource");
                String resourceFolder = System.IO.Path.Combine(solution.SolutionFolder, "resource");
                if (System.IO.Directory.Exists(resourceFolder))
                    System.IO.Directory.Delete(resourceFolder, true);
                System.IO.Directory.CreateDirectory(resourceFolder);
                DirectoryCopy.Copy(resource, resourceFolder, true);

                //apply resources
                new CodeFactory.Builder().Build(solution, true);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, scope);
            }
        }

        public Stream UploadData2(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UploadDataInternal(solution, messageBody, false);
                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadData2Async(Common.Solution solution, Stream messageBody)
        {
            return UploadDataAsync(solution, messageBody, false);
        }

        public Stream UploadData3(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UploadDataInternal(solution, messageBody, true);
                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UploadData3Async(Common.Solution solution, Stream messageBody)
        {
            return UploadDataAsync(solution, messageBody, true);
        }        

        private Stream UploadDataAsync(Common.Solution solution, Stream messageBody, bool checkExisting)
        {
            try
            {
                String contentEncoding = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.ContentEncoding];
                Guid sessionGuid = Guid.NewGuid();

                //save file
                string filepath = Path.Combine(System.IO.Path.GetTempPath(), sessionGuid.ToString());
                using (FileStream fs = System.IO.File.OpenWrite(filepath))
                {
                    messageBody.CopyTo(fs);
                    fs.Flush();
                }

                try
                {
                    //try to create session
                    CreateAyncUploadRecord(solution, sessionGuid);
                }
                catch
                {
                    try
                    {
                        System.IO.File.Delete(filepath);
                    }
                    catch
                    {
                    }
                    throw;
                }

                AsyncUploadThread t = new AsyncUploadThread(this,sessionGuid,solution,null,checkExisting, !String.IsNullOrEmpty(contentEncoding), contentEncoding, filepath);
                t.Start();
                if (t.IsStarted())
                    return Common.Utils.MakeTextAnswer(String.Format("{0}", sessionGuid.ToString()));
                else
                {
                    //t.Terminate();
                    return Common.Utils.MakeTextAnswer("Unable to start async upload session");
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public void UploadDataInternal(Common.Solution solution, Stream messageBody, bool checkExisting, bool zippedStream = true, String contentEncoding = null, String fileName = null, Action<int> progressCallback = null, int progressStep = 0)
        {
            System.IO.Stream tempStream = null;

            if (contentEncoding == null && zippedStream)
                contentEncoding = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.ContentEncoding];

            if (String.IsNullOrEmpty(contentEncoding) || !zippedStream)
            {
                if (!String.IsNullOrEmpty(fileName))
                    tempStream = System.IO.File.OpenRead(fileName);
                new DataUploader2(progressCallback, progressStep).UploadData(solution, tempStream!= null ? tempStream : messageBody, checkExisting);
            }
            else
            {
                switch (contentEncoding.ToLower())
                { 
                    case "gzip":
                        if (!String.IsNullOrEmpty(fileName))
                            tempStream = System.IO.File.OpenRead(fileName);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(tempStream != null ? tempStream : messageBody, System.IO.Compression.CompressionMode.Decompress, true))
                        {
                            gzip.CopyTo(ms);
                        }
                        ms.Position = 0;

                        new DataUploader2(progressCallback, progressStep).UploadData(solution, ms, checkExisting);

                        break;

                    case "deflate":
                        String tempFileName = null;

                        if (String.IsNullOrEmpty(fileName))
                        {
                            tempFileName = Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
                            using (FileStream fs = System.IO.File.OpenWrite(tempFileName))
                            {
                                messageBody.CopyTo(fs);
                                fs.Flush();
                            }
                        }

                        try
                        {
                            using (ZipArchive a = ZipFile.OpenRead(String.IsNullOrEmpty(fileName) ? tempFileName : fileName))
                            {
                                ZipArchiveEntry entry = a.Entries[0];
                                using (System.IO.Stream s = entry.Open())
                                {
                                    new DataUploader2(progressCallback, progressStep).UploadData(solution, s, checkExisting);
                                }
                            }
                        }
                        finally
                        {
                            if (!String.IsNullOrEmpty(tempFileName))
                                System.IO.File.Delete(tempFileName);
                        }

                        break;

                    default:
                        throw new Exception("Unsupported content type");
                }
            }

            if (tempStream != null)
                tempStream.Dispose();
        }

        public Stream DeploySolution(Common.Solution solution, Stream resourcesZipFileStream)
        {
            try
            {
                string filepath = Path.Combine(System.IO.Path.GetTempPath(), "resource.zip");
                using (FileStream tempZipFileStream = new FileStream(filepath, FileMode.Create))
                {
                    resourcesZipFileStream.CopyTo(tempZipFileStream);
                }

                FileHelper.CreateUnzipFolder(filepath, solution.SolutionFolder);

                ArchiveFileSystemInternal(solution);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream UpdateResources(Common.Solution solution)
        {
            try
            {
                new CodeFactory.Builder().Build(solution, true);
                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Common.UnsupportedCoreException e)
            {
                return Common.Utils.MakeTextAnswer(e.Message);
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream DownloadData(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UpdateCurrentCulterInfo();
                XmlDocument doc = new XmlDocument();
                doc.Load(Zip.UnzipStream(messageBody));
                Guid sessionId = new Guid(doc.DocumentElement.Attributes["Session"].Value);

                System.IO.MemoryStream response = new MemoryStream();

                SqlConnection conn = GetConnection(solution);
                using (conn)
                {
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        //start session
                        SqlCommand cmd = new SqlCommand("[admin].BeginSyncSession", conn, tran);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Parameters["@SessionId"].Value = sessionId;
                        cmd.ExecuteNonQuery();

                        StreamWriter wr = new StreamWriter(response);
                        wr.AutoFlush = true;

                        wr.WriteLine("<Root>");

                        //build response..
                        XmlNodeList entities = doc.DocumentElement.SelectNodes("//Request/Entity");
                        foreach (XmlNode entityNode in entities)
                        {
                            String entityName = entityNode.Attributes["Name"].Value;
                            String[] arr = entityName.Split('.');
                            entityName = String.Format("[{0}].[{1}]", arr[0], arr[1]);

                            cmd = new SqlCommand(String.Format("[{0}].[{1}_adm_getchanges]", arr[0], arr[1]), conn, tran);
                            cmd.CommandType = CommandType.StoredProcedure;
                            SqlCommandBuilder.DeriveParameters(cmd);
                            cmd.Parameters["@SessionId"].Value = sessionId;

                            XmlReader result = cmd.ExecuteXmlReader();
                            result.MoveToContent();
                            wr.Write(result.ReadOuterXml());
                        }
                        tran.Commit();

                        wr.WriteLine("</Root>");

                        response.Position = 0;
                        return Zip.ZipStream(response);
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream DownloadDataCommit(Common.Solution solution, Stream messageBody)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Zip.UnzipStream(messageBody));
                Guid sessionId = new Guid(doc.DocumentElement.Attributes["Session"].Value);

                SqlConnection conn = GetConnection(solution);
                using (conn)
                {
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        SqlCommand cmd = new SqlCommand("[admin].CommitSyncSession", conn, tran);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Parameters["@SessionId"].Value = sessionId;
                        cmd.ExecuteNonQuery();
                        tran.Commit();

                        return Common.Utils.MakeTextAnswer("ok");
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }

        }

        public Stream DownloadDeleted(Common.Solution solution)
        {
            try
            {
                System.IO.MemoryStream response = new MemoryStream();

                SqlConnection conn = GetConnection(solution);
                using (conn)
                {
                    StreamWriter wr = new StreamWriter(response);
                    wr.AutoFlush = true;

                    SqlCommand cmd = new SqlCommand("[admin].GetDeleted", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);

                    XmlReader result = cmd.ExecuteXmlReader();
                    result.MoveToContent();
                    wr.Write(result.ReadOuterXml());

                    response.Position = 0;
                    return Zip.ZipStream(response);
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream DownloadDeviceLog(Common.Solution solution)
        {
            try
            {
                System.IO.MemoryStream response = new MemoryStream();

                SqlConnection conn = GetConnection(solution);
                using (conn)
                {
                    SqlCommand cmd = new SqlCommand("[admin].GetDeviceLog", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        SqlCommandBuilder.DeriveParameters(cmd);
                    }
                    catch
                    {
                        throw new NotSupportedException();
                    }

                    StreamWriter wr = new StreamWriter(response);
                    wr.AutoFlush = true;

                    XmlReader result = cmd.ExecuteXmlReader();
                    result.MoveToContent();
                    wr.Write(result.ReadOuterXml());

                    response.Position = 0;
                    return Zip.ZipStream(response);
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream CheckIfExists(Common.Solution solution, Stream messageBody)
        {
            try
            {
                UpdateCurrentCulterInfo();
                XmlDocument doc = new XmlDocument();
                doc.Load(Zip.UnzipStream(messageBody));

                System.IO.MemoryStream response = new MemoryStream();

                SqlConnection conn = GetConnection(solution);
                using (conn)
                {
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        StreamWriter wr = new StreamWriter(response);
                        wr.AutoFlush = true;

                        wr.WriteLine("<Root>");

                        //build response..
                        XmlNodeList entities = doc.DocumentElement.SelectNodes("//Request/Entity");
                        foreach (XmlNode entityNode in entities)
                        {
                            String entityName = entityNode.Attributes["Name"].Value;
                            String[] arr = entityName.Split('.');
                            entityName = String.Format("[{0}].[{1}]", arr[0], arr[1]);

                            SqlCommand cmd = new SqlCommand(String.Format("[{0}].[{1}_adm_getnotexisting]", arr[0], arr[1]), conn, tran);
                            cmd.CommandType = CommandType.StoredProcedure;
                            SqlCommandBuilder.DeriveParameters(cmd);
                            cmd.Parameters["@Xml"].Value = entityNode.OuterXml;

                            XmlReader result = cmd.ExecuteXmlReader();
                            result.MoveToContent();
                            wr.Write(result.ReadOuterXml());
                        }
                        tran.Commit();
                        
                        wr.WriteLine("</Root>");

                        response.Position = 0;
                        return Zip.ZipStream(response);
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream GetAllStorageData(Common.Solution solution)
        {
            try
            {
                string folderForZip = solution.SolutionFolder;
                string zippingFilePath = System.IO.Path.GetTempPath() + "Solution.zip";
                FileHelper.CreateZipArchieve(folderForZip, zippingFilePath);

                FileStream fs = File.OpenRead(zippingFilePath);
                return fs;
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream GetClientDll(Common.Solution solution)
        {
            try
            {
                string pathToClientDll = Path.Combine(solution.SolutionFolder, @"bin\Client.dll");

                FileStream fs = File.OpenRead(pathToClientDll);
                return fs;
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream GetClientMetadata(Common.Solution solution)
        {
            try
            {
                string pathToClientDll = Path.Combine(solution.SolutionFolder, @"configuration\configuration.xml");

                FileStream fs = File.OpenRead(pathToClientDll);
                return fs;
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream ArchiveFileSystem(Common.Solution solution)
        {
            try
            {
                ArchiveFileSystemInternal(solution);
                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        public Stream AsyncTaskStatus(Common.Solution solution, String id)
        {
            try
            {
                Guid sessionId;
                if (!Guid.TryParse(id, out sessionId))
                    throw new Exception("Invalid task id");
                else
                {
                    return Common.Utils.MakeTextAnswer(CheckAsyncUploadRecord(solution, sessionId));
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, solution.Name);
            }
        }

        private void ArchiveFileSystemInternal(Common.Solution solution)
        {
            String tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            //we cant copy the entire "access" folder, "devices.txt" file should be excluded, so..
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(tempFolder, "access"));

            String passFile = System.IO.Path.Combine(solution.SolutionFolder, @"access\password.txt");
            if (System.IO.File.Exists(passFile))
                System.IO.File.Copy(passFile, System.IO.Path.Combine(tempFolder, @"access\password.txt"));

            String licFile = System.IO.Path.Combine(solution.SolutionFolder, @"access\licenses.txt");
            if (System.IO.File.Exists(licFile))
                System.IO.File.Copy(licFile, System.IO.Path.Combine(tempFolder, @"access\licenses.txt"));

            //DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "access"), System.IO.Path.Combine(tempFolder, "access"), true);
            DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "bin"), System.IO.Path.Combine(tempFolder, "bin"), true);
            DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "code"), System.IO.Path.Combine(tempFolder, "code"), true);
            DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "configuration"), System.IO.Path.Combine(tempFolder, "configuration"), true);
            DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "database"), System.IO.Path.Combine(tempFolder, "database"), true);
            //DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "log"), System.IO.Path.Combine(tempFolder, "log"), true);
            DirectoryCopy.Copy(System.IO.Path.Combine(solution.SolutionFolder, "resource"), System.IO.Path.Combine(tempFolder, "resource"), true);

            string zippingFilePath = tempFolder + "Solution.zip";
            FileHelper.CreateZipArchieve(tempFolder, zippingFilePath);

            UInt32 crc = Crc32.CRC(zippingFilePath);

            using (FileStream fs = new FileStream(zippingFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT CRC FROM [admin].[FileSystem] WHERE [Date] = (SELECT MAX([Date]) FROM [admin].[FileSystem])", conn))
                    {
                        object crcLast = cmd.ExecuteScalar();
                        if (crcLast != null)
                        {
                            if (crc.ToString().Equals(crcLast.ToString()))
                                return;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [admin].[FileSystem]([Date],[Data],[CRC]) VALUES(@Date,@Data,@CRC)", conn))
                    {
                        cmd.Parameters.AddWithValue("@CRC", crc.ToString());
                        cmd.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                        SqlParameter p = new SqlParameter("@Data", SqlDbType.VarBinary, bytes.Length, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Current, bytes);
                        cmd.Parameters.Add(p);
                        cmd.ExecuteNonQuery();
                    }
                }

            }
        }

        private SqlConnection GetConnection(Common.Solution solution)
        {
            SqlConnection conn = new SqlConnection(solution.ConnectionString);
            conn.Open();
            return conn;
        }

        private void UpdateCurrentCulterInfo()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("ru-RU");
        }

        private void LogException(Common.Solution solution, Exception e)
        {
            Common.Solution.LogException(solution.Name, "admin", e);
        }

        private bool LogRequestInfo(Common.Solution solution, DateTime startTime)
        {
            HttpRequestMessageProperty g = OperationContext.Current.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            System.Net.WebHeaderCollection headers = g.Headers;

            String msg = String.Format("StartTime:{1};Duration:{2}ms", startTime.ToShortDateString(), startTime.ToLongTimeString(), (DateTime.Now - startTime).Milliseconds.ToString());
            foreach (String key in g.Headers.Keys)
            {
                msg = msg + String.Format(";{0}:{1}", key, headers[key] == null ? "null" : headers[key]);
            }
            Common.Solution.Log(solution.Name, "admin", msg);
            return true;
        }


        private void CreateAyncUploadRecord(Common.Solution solution, Guid sessionId)
        {
            SqlConnection conn = GetConnection(solution);
            using (conn)
            {
                SqlCommand cmd = new SqlCommand("[admin].BeginAsyncUploadSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    SqlCommandBuilder.DeriveParameters(cmd);
                }
                catch
                {
                    throw new Exception("Database does not support async upload");
                }
                cmd.Parameters["@SessionId"].Value = sessionId;
                cmd.ExecuteNonQuery();
            }
        }

        public void CommitAsyncUploadRecord(Common.Solution solution, Guid sessionId, String status = "")
        {
            SqlConnection conn = GetConnection(solution);
            using (conn)
            {
                using (SqlCommand cmd = new SqlCommand("[admin].CommitAsyncUploadSession", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.Parameters["@SessionId"].Value = sessionId;
                    cmd.Parameters["@Status"].Value = String.IsNullOrEmpty(status) ? "ok" : status;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private String CheckAsyncUploadRecord(Common.Solution solution, Guid sessionId)
        {
            String result = "";
            SqlConnection conn = GetConnection(solution);
            using (conn)
            {
                SqlCommand cmd = new SqlCommand("[admin].CheckAsyncUploadSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@SessionId"].Value = sessionId;
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        result = r.GetString(0);
                    else
                        result = "Task not found";
                }
            }

            if (String.IsNullOrEmpty(result))
                return "processing";
            else
                return result;
        }

    }
}
