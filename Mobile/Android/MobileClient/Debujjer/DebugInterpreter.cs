using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BitMobile.Debugger
{
    // TODO: Дебагер я не трогал
    public class DebugInterpreter
    {
        static object lockObject = new object();
        const string locapath = "/debugger/";
        private string remoteHost;

        private Thread mainThread = null;

        public static DebugInterpreter interpreter = null;

        public static DebugInterpreter CreateInstance(bool waitDebugger)
        {
            if (interpreter == null)
            {
                interpreter = new DebugInterpreter();
                interpreter.Start(System.Threading.Thread.CurrentThread, waitDebugger);
            }
            return interpreter;
        }

        private DebugInterpreter()
        {
        }

        public void Start(Thread mainThread, bool waitDebugger)
        {
            this.mainThread = mainThread;
            Thread thread = new Thread(WaitCommand);

            thread.Start();
            
            if (waitDebugger)
                resumeEvent.WaitOne();
        }

        void WaitCommand(object obj)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:8082" + locapath);
                listener.Start();

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    remoteHost = String.Format("http://{0}", request.RemoteEndPoint.Address.ToString());
                    String method = null;

                    try
                    {
                        using (System.IO.StreamWriter wr = new StreamWriter(response.OutputStream))
                        {
                            response.StatusCode = 200;
                            try
                            {
                                method = request.Url.LocalPath.Remove(0, locapath.Length);
                                string query = request.Url.Query;

                                List<string> parameters = new List<string>();
                                if (!string.IsNullOrWhiteSpace(query))
                                {
                                    query = query.Remove(0, 1);
                                    foreach (string param in query.Split('&'))
                                    {
                                        string value = param.Split('=')[1];
                                        value = WebUtility.UrlDecode(value);
                                        value = ParseUnicodeString(value);
                                        parameters.Add(value);
                                    }
                                }

                                String result = Execute(method, parameters.ToArray());
                                WriteMessage(response.OutputStream, result);
                            }
                            catch (Exception e)
                            {
                                WriteMessage(response.OutputStream, e.Message);
                            }
                        }
                        response.Close();

                        if (method.ToLower().Equals("run"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            SendCommand(remoteHost, "started");
                        }
                    }
                    catch
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void WriteMessage(Stream stream, String message)
        {
            using (System.IO.StreamWriter wr = new StreamWriter(stream))
            {
                wr.Write(message);
                wr.Flush();
            }
        }

        private string ParseUnicodeString(string s)
        {
            Dictionary<string, string> table = new Dictionary<string, string>();

            Regex r = new Regex("%u([0-9A-Fa-f]){4}");
            MatchCollection matches = r.Matches(s);
            foreach (Match m in matches)
            {
                string value = m.Value;
                if (!table.ContainsKey(value))
                {
                    short code = short.Parse(value.Remove(0, 2), NumberStyles.HexNumber);
                    table.Add(value, Encoding.Unicode.GetString(new byte[] { (byte)code, (byte)(code >> 8) }));
                }
            }

            StringBuilder sb = new StringBuilder(s);
            foreach (var kvp in table)
            {
                sb.Replace(kvp.Key, kvp.Value);
            }

            return sb.ToString();
        }

        private static String SendCommand(String host, String command)
        {
            String result;
            var ub = new UriBuilder(String.Format(@"{0}:8082/debugger/{1}", host, command));
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ub.Uri);
            req.Method = "GET";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                using (System.IO.StreamReader r = new StreamReader(resp.GetResponseStream()))
                {
                    result = r.ReadToEnd();
                }
                resp.Close();
                return result;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    if (e.Response is HttpWebResponse)
                        throw new Exception(((HttpWebResponse)e.Response).StatusDescription);
                }
                throw;
            }
        }

        public String Execute(string method, string[] parameters)
        {
            String result = null;
            try
            {
                MethodInfo mi = this.GetType().GetMethod(string.Format("Do{0}", method), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
                if (mi != null)
                    result = (String)mi.Invoke(this, new object[] { parameters });
                else
                    result = string.Format("invalid command", method);

            }
            catch (Exception e)
            {
                result = string.Format("Error: {0}", e.Message);
            }

            return result.ToString();
        }


        //-------------------------------------------------COMMANDS-------------------------------------------------------

        private object EvaluateExpression(String[] parameters)
        {
            String[] arr = parameters[0].Split('.');
            var root = CurrentBreakpoint.Locals[arr[0]];
            if (root == null)
                return null;
            object rootValue = root.Value;

            if (arr.Length == 1)
                return rootValue;

            BitMobile.ValueStack.IEvaluator evaluator = CurrentBreakpoint.Locals["$"].Value as BitMobile.ValueStack.IEvaluator;

            return evaluator.Evaluate(parameters[0], rootValue);
        }

        private String GetObjectFields(object obj)
        {
            StringBuilder sb = new StringBuilder();

            if (obj is Dictionary<String, object>)
            {
                Dictionary<String, object> d = (Dictionary<String, object>)obj;
                foreach (String s in d.Keys)
                {
                    object v = d[s];
                    sb.AppendLine(s + ":" + (v == null ? "null" : v.ToString()));
                }
                return sb.ToString();
            }

            BitMobile.ValueStack.IEvaluator evaluator = CurrentBreakpoint.Locals["$"].Value as BitMobile.ValueStack.IEvaluator;

            System.Reflection.PropertyInfo[] pis = obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo pi in pis)
            {
                object v = evaluator.Evaluate("." + pi.Name, obj);
                sb.AppendLine(pi.Name + ":" + (v == null ? "null" : v.ToString()));
            }

            return sb.ToString();
        }

        public String DoEvaluate(string[] parameters)
        {
            if (CurrentBreakpoint == null)
                return "not found";
            else
            {
                if (parameters.Length != 1)
                    return "not found";
                else
                {
                    object value = EvaluateExpression(parameters);
                    return value == null ? "null" : value.ToString();
                }
            }
        }

        public String DoGetFields(string[] parameters)
        {
            if (CurrentBreakpoint == null)
                return "not found";
            else
            {
                if (parameters.Length != 1)
                    return "not found";
                else
                {
                    object value = EvaluateExpression(parameters);
                    if (value == null)
                        return "";
                    else
                        return GetObjectFields(value);
                }
            }
        }

        public String DoRun(string[] parameters)
        {
            return "";
        }

        public String DoResume(string[] parameters)
        {
            if (CurrentBreakpoint != null)
                CurrentBreakpoint = null;
            resumeEvent.Set();
            return "";
        }

        public String DoSetBreakpoint(string[] parameters)
        {
            if (parameters.Length != 1)
                return "bad arguments";
            else
            {
                String[] args = parameters[0].Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length != 2)
                    return "bad arguments";
                else
                {
                    String module = GetModuleName(args[0]);
                    int line = int.Parse(args[1]);
                    List<int> points = null;
                    if (!breakPoints.TryGetValue(module, out points))
                        breakPoints.Add(module, new List<int>() { line });
                    else
                    {
                        if (points.Contains(line))
                            points.Remove(line);
                        else
                            points.Add(line);
                    }
                    return "ok";
                }
            }
        }

        public String DoClearBreakpoint(string[] parameters)
        {
            if (parameters.Length != 1)
                return "bad arguments";
            else
            {
                String[] args = parameters[0].Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length != 2)
                    return "bad arguments";
                else
                {
                    String module = GetModuleName(args[0]);
                    int line = int.Parse(args[1]);
                    List<int> points = null;
                    if (breakPoints.TryGetValue(module, out points))
                    {
                        if (points.Contains(line))
                            points.Remove(line);
                    }
                    return "ok";
                }
            }
        }

        public String DoTerminate(string[] parameters)
        {
            mainThread.Abort();
            throw new Exception("Application terminated");
        }

        private String GetModuleName(String name)
        {
            String result = "";
            String[] arr = name.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length < 4)
                throw new Exception(String.Format("Invalid module name '{0}'", name));
            for (int i = 3; i < arr.Length; i++)
            {
                if (!String.IsNullOrEmpty(result))
                    result = result + "\\";
                result = result + arr[i].ToLower();
            }

            if (!breakPointsMap.ContainsKey(result))
                breakPointsMap.Add(result, name);

            return result;
        }

        //------------------------------------------------------------------------------------------------------

        private System.Threading.AutoResetEvent resumeEvent = new AutoResetEvent(false);
        
        private Jint.Debugger.DebugInformation CurrentBreakpoint = null;
        private Dictionary<String, List<int>> breakPoints = new Dictionary<string, List<int>>();
        private Dictionary<String, String> breakPointsMap = new Dictionary<string,string>();

        public void OnBreak(object sender, EventArgs e)
        {
            CurrentBreakpoint = (Jint.Debugger.DebugInformation)e;
            String moduleName = breakPointsMap[CurrentBreakpoint.Module.ToLower()];

            SendCommand(remoteHost, String.Format("suspended?breakpoint={0}:{1}", moduleName, CurrentBreakpoint.CurrentStatement.Source.Start.Line.ToString()));
            resumeEvent.WaitOne();
        }

        public int[] GetBreakPoints(String moduleName)
        {
            moduleName = moduleName.ToLower();

            if (breakPoints.ContainsKey(moduleName))
                return breakPoints[moduleName].ToArray();
            else
                return null;
        }

    }
}
