using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BitMobile.Common.ScriptEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.Debugger
{
    // TODO: Дебагер я не трогал
    public class DebugInterpreter
    {
        const string Locapath = "/debugger/";
        private string _remoteHost;
        private Thread _mainThread;
        private static DebugInterpreter _interpreter;

        public static DebugInterpreter CreateInstance()
        {
            if (_interpreter == null)
            {
                _interpreter = new DebugInterpreter();
                _interpreter.Start(Thread.CurrentThread);
            }
            return _interpreter;
        }
        
        private void Start(Thread mainThread)
        {
            _mainThread = mainThread;
            var thread = new Thread(WaitCommand);
            thread.IsBackground = true;
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            try
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://+:8082" + Locapath);
                listener.Start();

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    _remoteHost = string.Format("http://{0}", request.RemoteEndPoint.Address);
                    string method = null;

                    try
                    {
                        using (var wr = new StreamWriter(response.OutputStream))
                        {
                            response.StatusCode = 200;
                            try
                            {
                                method = request.Url.LocalPath.Remove(0, Locapath.Length);
                                string query = request.Url.Query;

                                var parameters = new List<string>();
                                if (!string.IsNullOrWhiteSpace(query))
                                {
                                    query = query.Remove(0, 1);
                                    // ReSharper disable once LoopCanBeConvertedToQuery
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

                        Debug.Assert(method != null, "method != null");
                        if (method.ToLower().Equals("run"))
                        {
                            Thread.Sleep(1000);
                            SendCommand(_remoteHost, "started");
                        }
                    }
                    catch
                    {
                        try
                        {
                            response.Close();
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch { }
                    }
                }
            }
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private void WriteMessage(Stream stream, String message)
        {
            using (var wr = new StreamWriter(stream))
            {
                wr.Write(message);
                wr.Flush();
            }
        }

        private string ParseUnicodeString(string s)
        {
            var table = new Dictionary<string, string>();

            var r = new Regex("%u([0-9A-Fa-f]){4}");
            MatchCollection matches = r.Matches(s);
            foreach (Match m in matches)
            {
                string value = m.Value;
                if (!table.ContainsKey(value))
                {
                    short code = short.Parse(value.Remove(0, 2), NumberStyles.HexNumber);
                    table.Add(value, Encoding.Unicode.GetString(new[] { (byte)code, (byte)(code >> 8) }));
                }
            }

            var sb = new StringBuilder(s);
            foreach (var kvp in table)
                sb.Replace(kvp.Key, kvp.Value);

            return sb.ToString();
        }

        private static string SendCommand(String host, String command)
        {
            var ub = new UriBuilder(String.Format(@"{0}:8082/debugger/{1}", host, command));
            var req = (HttpWebRequest)WebRequest.Create(ub.Uri);
            req.Method = "GET";
            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                String result;
                using (var r = new StreamReader(resp.GetResponseStream()))
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
                    var webResponse = e.Response as HttpWebResponse;
                    if (webResponse != null)
                        throw new Exception(webResponse.StatusDescription);
                }
                throw;
            }
        }

        public String Execute(string method, string[] parameters)
        {
            String result;
            try
            {
                MethodInfo mi = GetType().GetMethod(string.Format("Do{0}", method), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
                if (mi != null)
                    result = (String)mi.Invoke(this, new object[] { parameters });
                else
                    result = string.Format("invalid command {0}", method);

            }
            catch (Exception e)
            {
                result = string.Format("Error: {0}", e.Message);
            }

            return result;
        }


        //-------------------------------------------------COMMANDS-------------------------------------------------------

        private object EvaluateExpression(String[] parameters)
        {
            String[] arr = parameters[0].Split('.');
            var root = _currentBreakpoint.LocalValues[arr[0]];
            if (root == null)
                return null;
            
            if (arr.Length == 1)
                return root;

            var evaluator = (IValueStack)_currentBreakpoint.LocalValues["$"];

            return evaluator.Evaluate(parameters[0]);
        }

        private String GetObjectFields(object obj)
        {
            var sb = new StringBuilder();

            var dictionary = obj as Dictionary<string, object>;
            if (dictionary != null)
            {
                Dictionary<String, object> d = dictionary;
                foreach (String s in d.Keys)
                {
                    object v = d[s];
                    sb.AppendLine(s + ":" + (v == null ? "null" : v.ToString()));
                }
                return sb.ToString();
            }

            var evaluator = (IValueStack)_currentBreakpoint.LocalValues["$"];

            var pis = obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo pi in pis)
            {
                object v = evaluator.Evaluate("." + pi.Name/*, obj*/);
                sb.AppendLine(pi.Name + ":" + (v == null ? "null" : v.ToString()));
            }

            return sb.ToString();
        }

        public string DoEvaluate(string[] parameters)
        {
            if (_currentBreakpoint == null)
                return "not found";
            if (parameters.Length != 1)
                return "not found";
            object value = EvaluateExpression(parameters);
            return value == null ? "null" : value.ToString();
        }

        public string DoGetFields(string[] parameters)
        {
            throw new NotImplementedException("already");

            if (_currentBreakpoint == null)
                return "not found";
            if (parameters.Length != 1)
                return "not found";
            object value = EvaluateExpression(parameters);
            if (value == null)
                return "";
            return GetObjectFields(value);
        }

        public String DoRun(string[] parameters)
        {
            return "";
        }

        public String DoResume(string[] parameters)
        {
            _currentBreakpoint = null;
            _resumeEvent.Set();
            return "";
        }

        public String DoSetBreakpoint(string[] parameters)
        {
            if (parameters.Length != 1)
                return "bad arguments";
            String[] args = parameters[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2)
                return "bad arguments";
            String module = GetModuleName(args[0]);
            int line = int.Parse(args[1]);
            List<int> points;
            if (!_breakPoints.TryGetValue(module, out points))
                _breakPoints.Add(module, new List<int>() { line });
            else
            {
                if (points.Contains(line))
                    points.Remove(line);
                else
                    points.Add(line);
            }
            return "ok";
        }

        public String DoClearBreakpoint(string[] parameters)
        {
            if (parameters.Length != 1)
                return "bad arguments";
            String[] args = parameters[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2)
                return "bad arguments";
            String module = GetModuleName(args[0]);
            int line = int.Parse(args[1]);
            List<int> points;
            if (_breakPoints.TryGetValue(module, out points))
            {
                if (points.Contains(line))
                    points.Remove(line);
            }
            return "ok";
        }

        public String DoTerminate(string[] parameters)
        {
            _mainThread.Abort();
            throw new Exception("Application terminated");
        }

        private String GetModuleName(String name)
        {
            String result = "";
            String[] arr = name.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length < 4)
                throw new Exception(String.Format("Invalid module name '{0}'", name));
            for (int i = 3; i < arr.Length; i++)
            {
                if (!String.IsNullOrEmpty(result))
                    result = result + "\\";
                result = result + arr[i].ToLower();
            }

            if (!_breakPointsMap.ContainsKey(result))
                _breakPointsMap.Add(result, name);

            return result;
        }

        //------------------------------------------------------------------------------------------------------

        private readonly AutoResetEvent _resumeEvent = new AutoResetEvent(false);
        private readonly Dictionary<String, List<int>> _breakPoints = new Dictionary<string, List<int>>();
        private readonly Dictionary<String, String> _breakPointsMap = new Dictionary<string, string>();
        private IDebugInformation _currentBreakpoint;

        public void OnBreak(object sender, EventArgs e)
        {
            _currentBreakpoint = (IDebugInformation)e;
            String moduleName = _breakPointsMap[_currentBreakpoint.Module.ToLower()];

            SendCommand(_remoteHost, String.Format("suspended?breakpoint={0}:{1}"
                , moduleName, _currentBreakpoint.Line));
            _resumeEvent.WaitOne();
        }

        public int[] GetBreakPoints(String moduleName)
        {
            moduleName = moduleName.ToLower();

            if (_breakPoints.ContainsKey(moduleName))
                return _breakPoints[moduleName].ToArray();
            return null;
        }

    }
}
