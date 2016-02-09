using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    class Utils
    {
        static String ver = "2.378";
        static Dictionary<String, CommandInfo> commands = new Dictionary<string, CommandInfo>();
        static int errorCode = 0;

        static int Main(string[] args)
        {
            commands.Add("-h", new CommandInfo("", new String[] { }));
            commands.Add("-ver", new CommandInfo("display server version", new String[] { "-host", "-p" }));
            commands.Add("-li", new CommandInfo("display server licenses", new String[] { "-host", "-p" }));
            commands.Add("-la", new CommandInfo("activate license", new String[] { "-host", "-p", "file" }));
            commands.Add("-sl", new CommandInfo("display list of solutions from the given host", new String[] { "-host", "-p" }));
            commands.Add("-sc", new CommandInfo("create new solution", new String[] { "-host", "-p", "-sn" }));
            commands.Add("-sm", new CommandInfo("move existing solution", new String[] { "-host", "-p", "-sn", "-snn" }));
            commands.Add("-ssp", new CommandInfo("set solution password", new String[] { "-host", "-p", "-sn", "-sp" }));
            commands.Add("-sgp", new CommandInfo("get solution password", new String[] { "-host", "-p", "-sn" }));
            commands.Add("-slic", new CommandInfo("set solution licenses", new String[] { "-host", "-p", "-sn", "-slic" }));
            commands.Add("-sr", new CommandInfo("delete solution", new String[] { "-host", "-p", "-sn" }));

            commands.Add("-sda", new CommandInfo("extract last solution archive from the database", new String[] { "-database", "file" })); //?? -p or -sp ??
            commands.Add("-afs", new CommandInfo("archive solution into database", new String[] { "-host", "-sp", "-sn" }));
            commands.Add("-sd", new CommandInfo("download solution into local file", new String[] { "-host", "-sp", "-sn", "file" })); //?? -p or -sp ??
            commands.Add("-dm", new CommandInfo("deploy metadata file", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-dmf", new CommandInfo("deploy metadata file, only filters update", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-dma", new CommandInfo("deploy metadata file asynchronously", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-dm2", new CommandInfo("deploy metadata file (deprecated, no tabular section key check)", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-dma2", new CommandInfo("deploy metadata file asynchronously (deprecated, no tabular section key check)", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-rdm", new CommandInfo("redeploy metadata", new String[] { "-host", "-sp", "-sn" })); 
            commands.Add("-dr", new CommandInfo("deploy resources catalog", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-ar", new CommandInfo("apply resources on solution database", new String[] { "-host", "-sp", "-sn" }));
            commands.Add("-ud", new CommandInfo("upload data file", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-ud3", new CommandInfo("upload data file (use uploaddata3 server method)", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-uda", new CommandInfo("upload data file asynchronously", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-uda3", new CommandInfo("upload data file asynchronously (use uploaddata3 server method)", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-msp", new CommandInfo("make solution package", new String[] { "file1", "file2", "file3", "file" }));
            commands.Add("-dsp", new CommandInfo("deploy solution package", new String[] { "-host", "-sp", "-sn", "file" }));
            commands.Add("-ats", new CommandInfo("async task status", new String[] { "-host", "-sp", "-sn", "-atid" }));
            commands.Add("-af", new CommandInfo("archive file", new String[] { "file1", "file2" }));

            //commands.Add("-console", new CommandInfo("attach to device console", new String[] { "-host" }));

            if (args.Length == 0)
            {
                Console.WriteLine(String.Format("BitMobile server utility (ver {0})", ver));
                Console.WriteLine("No parameters given. Use -h for help");
                return 0;
            }

            String cmd = args[0];

            try
            {
                Dictionary<String, String> ps = ParseArguments(args);

                System.Reflection.MethodInfo mi = typeof(Utils).GetMethod(String.Format("Do{0}", cmd.Substring(1).ToUpper()), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (mi == null)
                    throw new Exception("Invalid arguments. Use -h for help.");
                mi.Invoke(null, new object[] { ps });

            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Console.WriteLine(e.InnerException.Message);
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return errorCode;
        }

        private static void DoH(Dictionary<String, String> args)
        {
            Console.WriteLine(String.Format("BitMobile server utility (ver {0})", ver));
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            foreach (KeyValuePair<String, CommandInfo> entry in commands)
            {
                if (!String.IsNullOrEmpty(entry.Value.Description))
                {
                    String ps = "";
                    foreach (String s in entry.Value.Arguments)
                    {
                        ps = ps + " " + s;
                    }
                    Console.WriteLine(String.Format("{0}{1}\t{2}", entry.Key, ps, entry.Value.Description));
                }
            }

            Console.WriteLine();
            Console.WriteLine("Keys:");
            Console.WriteLine("-host\tBitMobile server address, for example -host http://smth.cloudapp.net");
            Console.WriteLine("-p\tRoot password, for example -p parol123");
            Console.WriteLine("-sn\tSolution name, for example -sn customer12");
            Console.WriteLine("-sp\tSolution password, for example -sp pass12");
            Console.WriteLine("-slic\tSolution licenses, for example -slic 10");
            Console.WriteLine("-snn\tSolution name new, for example -snn customer12");
            Console.WriteLine("-atid\tAsync taskid, for example -atid 819d4fee-0e4d-4c60-bacb-9af0d4c9c5d2");
            Console.WriteLine("file\tFull path to local file or folder");
        }

        private static void DoVER(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/version", args["-host"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoLI(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/licenses", args["-host"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoLA(Dictionary<String, String> args)
        {
            Console.Write("License will be activated. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/system/licenses/add", args["-host"]);
                Stream resp = DoCommand(uri, args["-p"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }

        private static void DoSL(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions", args["-host"]);
            Stream resp = DoCommand(uri, args["-p"]);
            String result = StringFromStream(resp);
            foreach(String s in result.Split(';'))
                Console.WriteLine(s);
        }

        private static void DoSC(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/create/{1}", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoSM(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/move/{1}/{2}", args["-host"], args["-sn"], args["-snn"]);
            Stream resp = DoCommand(uri, args["-p"]);
            var s = StringFromStream(resp);
            Console.WriteLine(s);
            errorCode = s == "ok" ? 0 : 1;
        }

        private static void DoSSP(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/setpassword/{1}/{2}", args["-host"], args["-sn"], args["-sp"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoSGP(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/getpassword/{1}", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoSLIC(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/setlicenses/{1}/{2}", args["-host"], args["-sn"], args["-slic"]);
            Stream resp = DoCommand(uri, args["-p"]);
            var s = StringFromStream(resp);
            Console.WriteLine(s);
            errorCode = s == "ok" ? 0 : 1;
        }

        private static void DoSR(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/system/solutions/remove/{1}", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-p"]);
            ProcessResponse(resp);
        }

        private static void DoSD(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/GetAllStorageData", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-sp"]);
            using (FileStream tempZipFileStream = new FileStream(args["file"], FileMode.Create))
            {
                resp.CopyTo(tempZipFileStream);
            }
            Console.WriteLine("ok");
        }

        private static void DoSDA(Dictionary<String, String> args)
        {
            using (SqlConnection cn = new SqlConnection(args["-database"]))
            using (SqlCommand cmd = new SqlCommand("SELECT [Data] FROM [admin].[FileSystem] WHERE [Date] = (SELECT MAX([Date]) FROM [admin].[FileSystem])", cn))
            {
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                using (FileStream fs = new FileStream(args["file"], FileMode.Create, FileAccess.Write))
                {
                    if (dr.Read())
                    {
                        byte[] b = new byte[(dr.GetBytes(0, 0, null, 0, int.MaxValue))];
                        dr.GetBytes(0, 0, b, 0, b.Length);
                        fs.Write(b, 0, b.Length);
                    }
                }
            }
            Console.WriteLine("ok");
        }

        private static void DoAFS(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/ArchiveFileSystem", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-sp"]);
            ProcessResponse(resp);
        }

        private static void DoDM(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/UploadMetadata2", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }

        private static void DoDMF(Dictionary<String, String> args)
        {
            Console.Write("Existing filters will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/UploadMetadataFiltersOnly", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }

        private static void DoDMA(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/UploadMetadata2Async", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }

        private static void DoDMA2(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/UploadMetadataAsync", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }
        private static void DoDM2(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/UploadMetadata", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST");
                ProcessResponse(resp);
            }
        }

        private static void DoRDM(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/ReDeployMetadata", args["-host"], args["-sn"]);
                Stream resp = DoCommand(uri, args["-sp"]);
                ProcessResponse(resp);
            }
        }

        private static void DoDR(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/DeploySolution", args["-host"], args["-sn"]);

            String archive = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            ZipFile.CreateFromDirectory(args["file"], archive, CompressionLevel.Optimal, false, Encoding.UTF8);
            try
            {
                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(archive), "POST");
                ProcessResponse(resp);
            }
            finally
            {
                if (System.IO.File.Exists(archive))
                    System.IO.File.Delete(archive);
            }
        }

        private static void DoAR(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/UpdateResources", args["-host"], args["-sn"]);
            Stream resp = DoCommand(uri, args["-sp"]);
            ProcessResponse(resp);
        }

        private static void DoUD(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/UploadData2", args["-host"], args["-sn"]);

            Stream resp = DoCommand(uri, args["-sp"], ArchiveFile(args["file"]), "POST", GetGzipHeaders());
            ProcessResponse(resp);
        }

        private static void DoUD3(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/UploadData3", args["-host"], args["-sn"]);

            Stream resp = DoCommand(uri, args["-sp"], ArchiveFile(args["file"]), "POST", GetGzipHeaders());
            ProcessResponse(resp);
        }

        private static void DoUDA(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/UploadData2Async", args["-host"], args["-sn"]);

            Stream resp = DoCommand(uri, args["-sp"], ArchiveFile(args["file"]), "POST", GetGzipHeaders());
            ProcessResponse(resp);
        }

        private static void DoUDA3(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/UploadData3Async", args["-host"], args["-sn"]);

            Stream resp = DoCommand(uri, args["-sp"], ArchiveFile(args["file"]), "POST", GetGzipHeaders());
            Console.WriteLine(StringFromStream(resp));
        }

        private static void DoMSP(Dictionary<String, String> args)
        {
            String archive = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(archive);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(archive,"resource"));

            System.IO.File.Copy(args["file1"], System.IO.Path.Combine(archive, "metadata.xml"));
            System.IO.File.Copy(args["file2"], System.IO.Path.Combine(archive, "data.xml"));
            DirectoryCopy.Copy(args["file3"], System.IO.Path.Combine(archive, "resource"), true);

            ZipFile.CreateFromDirectory(archive, args["file"] , CompressionLevel.Optimal, false, Encoding.UTF8);
        }

        private static void DoDSP(Dictionary<String, String> args)
        {
            Console.Write("Existing database will be removed. Do you want to proceed ?");
            ConsoleKeyInfo info = Console.ReadKey(true);
            Console.WriteLine("");
            if (info.KeyChar == 'y' || info.KeyChar == 'Y')
            {
                String uri = String.Format("{0}/{1}/admin/DeploySolutionPackage", args["-host"], args["-sn"]);

                Stream resp = DoCommand(uri, args["-sp"], System.IO.File.ReadAllBytes(args["file"]), "POST", GetGzipHeaders());
                ProcessResponse(resp);
            }
        }

        private static void DoATS(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/{1}/admin/AsyncTaskStatus/{2}", args["-host"], args["-sn"], args["-atid"]);

            Stream resp = DoCommand(uri, args["-sp"]);
            Console.WriteLine(StringFromStream(resp));
        }

        private static void DoAF(Dictionary<String, String> args)
        {
            String archive = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(archive);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(archive, "resource"));

            System.IO.File.Copy(args["file1"], System.IO.Path.Combine(archive, "metadata.xml"));
            System.IO.File.Copy(args["file2"], System.IO.Path.Combine(archive, "data.xml"));
            DirectoryCopy.Copy(args["file3"], System.IO.Path.Combine(archive, "resource"), true);

            ZipFile.CreateFromDirectory(archive, args["file"], CompressionLevel.Optimal, false, Encoding.UTF8);
        }

        private static void DoCONSOLE(Dictionary<String, String> args)
        {
            throw new NotImplementedException();
            //DeviceConsole.Attach(args["-host"]);
        }

        private static Dictionary<String, String> ParseArguments(String[] args)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            String cmd = GetArg(args, 0).ToLower();
            if(!commands.ContainsKey(cmd))
                throw new Exception("Invalid arguments. Use -h for help.");

            String[] ps = commands[cmd].Arguments;
            int idx = 1;
            foreach (String p in ps)
            {
                if (p.StartsWith("file"))
                {
                    result.Add(p, GetArg(args, idx));
                    idx++;
                }
                else
                {
                    if (!GetArg(args, idx).ToLower().Equals(p))
                        throw new Exception("Invalid arguments. Use -h for help.");

                    result.Add(p, GetArg(args, idx + 1));
                    idx += 2;
                }
            }

            return result;
        }

        private static String GetArg(String[] args, int idx)
        {
            if(idx>=args.Length)
                throw new Exception("Invalid arguments. Use -h for help.");
            return args[idx];
        }

        private static System.IO.Stream DoCommand(String uri, String p, byte[] data = null, String method = "GET", Dictionary<HttpRequestHeader,String> headers = null)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Timeout = 300000; //5 min
            req.Method = method;
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("admin" + ":" + p));
            req.Headers.Add("Authorization", "Basic " + svcCredentials);
            if (headers != null)
            {
                foreach (var entry in headers)
                {
                    req.Headers.Add(entry.Key, entry.Value);
                }
            }

            if (data != null && method.Equals("POST"))
            {
                using (Stream requestStream = req.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }
            }

            WebResponse resp = req.GetResponse();
            return resp.GetResponseStream();
        }

        private static void ProcessResponse(Stream resp)
        {
            var s = StringFromStream(resp);
            Console.WriteLine(s);
            errorCode = s == "ok" ? 0 : 1;
        }

        private static String StringFromStream(Stream s)
        {
            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        private static byte[] ArchiveFile(String fileName)
        {
            System.IO.MemoryStream ms = new MemoryStream();

            using(System.IO.Stream input = File.OpenRead(fileName))
            {
                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                {
                    input.CopyTo(gzip);
                }
            }
            ms.Position = 0;

            using (System.IO.BinaryReader r = new BinaryReader(ms))
            {
                return r.ReadBytes((int)ms.Length);
            }
        }

        private static Dictionary<HttpRequestHeader, String> GetGzipHeaders()
        {
            Dictionary<HttpRequestHeader, String> result = new Dictionary<HttpRequestHeader, string>();
            result.Add(HttpRequestHeader.ContentEncoding, "gzip");
            return result;
        }
    }

    class CommandInfo
    {
        public String Description { get; private set; }
        public String[] Arguments { get; private set; }

        public CommandInfo(String description, String[] arguments)
        {
            this.Description = description;
            this.Arguments = arguments;
        }
    }

}
