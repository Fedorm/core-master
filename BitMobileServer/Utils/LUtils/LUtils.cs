using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Utils
{
    class Utils
    {
        static String ver = "1.02";
        static Dictionary<String, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        static HttpStatusCode lastStatusCode = 0;
        
        static int Main(string[] args)
        {
            commands.Add("-h", new CommandInfo("", new String[] { }));
            commands.Add("-lsi", new CommandInfo("init license server", new String[] { "-host", "-p" }));
            commands.Add("-lc", new CommandInfo("create lisence", new String[] { "-host", "-p", "-ln", "-lqty", "-le", "file" }));

            if (args.Length == 0)
            {
                Console.WriteLine(String.Format("BitMobile license server utility (ver {0})", ver));
                Console.WriteLine("No parameters given. Use -h for help");
                return 1;
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
            return lastStatusCode == HttpStatusCode.OK ? 0 : 1;
        }

        private static void DoH(Dictionary<String, String> args)
        {
            Console.WriteLine(String.Format("BitMobile license server utility (ver {0})", ver));
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
            Console.WriteLine("-ln\tLicense name, for example -ln customer12");
            Console.WriteLine("-le\tLicense expiration date, for example -le 31.12.2018");
            Console.WriteLine("file\tFull path to local file or folder");
        }

        private static void DoLSI(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/license/initservice", args["-host"]);
            Stream resp = DoCommand(uri, args["-p"]);
            Console.WriteLine(StringFromStream(resp));
        }

        private static void DoLC(Dictionary<String, String> args)
        {
            String uri = String.Format("{0}/license/createlicense?ln={1}&lqty={2}&le={3}", args["-host"], args["-ln"], args["-lqty"], args["-le"]);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                
            Stream resp = DoCommand(uri, args["-p"]);
            if (lastStatusCode == HttpStatusCode.OK)
            {
                using (FileStream fs = new FileStream(args["file"], FileMode.Create))
                {
                    resp.CopyTo(fs);
                }
                Console.WriteLine("ok");
            }
            else
                Console.WriteLine(StringFromStream(resp));
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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(HttpUtility.UrlPathEncode(uri));
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

            lastStatusCode = ((HttpWebResponse)resp).StatusCode;

            return resp.GetResponseStream();
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
