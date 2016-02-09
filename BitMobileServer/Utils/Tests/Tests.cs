using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Tests
    {
        static String ver = "1.0";
        static Dictionary<String, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        static void Main(string[] args)
        {
            commands.Add("-h", new CommandInfo("", new String[] { }));
            commands.Add("-r", new CommandInfo("run test", new String[] { "-host", "-ep", "file", "report", "resources" }));

            if (args.Length == 0)
            {
                System.Console.WriteLine("No parameters given. Use -h for help");
                return;
            }

            String cmd = args[0];

            try
            {
                Dictionary<String, String> ps = ParseArguments(args);

                System.Reflection.MethodInfo mi = typeof(Tests).GetMethod(String.Format("Do{0}", cmd.Substring(1).ToUpper()), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (mi == null)
                    throw new Exception("Invalid arguments. Use -h for help.");
                mi.Invoke(null, new object[] { ps });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                System.Console.WriteLine(e.InnerException.Message);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private static void DoH(Dictionary<String, String> args)
        {
            System.Console.WriteLine(String.Format("BitMobile tests utility (ver {0})", ver));
            System.Console.WriteLine();
            System.Console.WriteLine("Arguments:");
            foreach (KeyValuePair<String, CommandInfo> entry in commands)
            {
                if (!String.IsNullOrEmpty(entry.Value.Description))
                {
                    String ps = "";
                    foreach (String s in entry.Value.Arguments)
                    {
                        ps = ps + " " + s;
                    }
                    System.Console.WriteLine(String.Format("{0}{1}\t{2}", entry.Key, ps, entry.Value.Description));
                }
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Keys:");
            System.Console.WriteLine("-host\tTarget device IP address, for example -host http://192.168.0.100:8088");
            System.Console.WriteLine("-host\tEntry point function name, for example -ep main");
            System.Console.WriteLine("file\tFull path to test file");
            System.Console.WriteLine("report\tFull path to report file");
            System.Console.WriteLine("resources\tFull path to resource directory");
        }

        private static void DoR(Dictionary<String, String> args)
        {
            object result = Script.RunTest(args["-host"], args["-ep"], args["file"], args["report"], args["resources"]);
        }

        private static Dictionary<String, String> ParseArguments(String[] args)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            String cmd = GetArg(args, 0).ToLower();
            if (!commands.ContainsKey(cmd))
                throw new Exception("Invalid arguments. Use -h for help.");

            String[] ps = commands[cmd].Arguments;
            int idx = 1;
            foreach (String p in ps)
            {
                if (p.Equals("file") || p.Equals("report") || p.Equals("resources"))
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
            if (idx >= args.Length)
                throw new Exception("Invalid arguments. Use -h for help.");
            return args[idx];
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
