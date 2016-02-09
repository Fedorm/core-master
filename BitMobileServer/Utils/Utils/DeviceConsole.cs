using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Utils
{
    public static class DeviceConsole
    {
        const string locapath = "/debugger/";
        private static bool suspended = false;

        private static Queue<String> messages = new Queue<string>();

        private static System.Threading.Thread consoleThread;
        private static System.Threading.Thread callbackThread;

        public static void Attach(String host)
        {
            consoleThread = new System.Threading.Thread(WaitConsoleData);
            consoleThread.Start(host);

            callbackThread = new System.Threading.Thread(WaitCallback);
            callbackThread.Start(host);

            while (true)
            {
                String cmd = Console.ReadLine();
                if (!String.IsNullOrEmpty(cmd))
                {
                    try
                    {
                        String[] arr = cmd.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        List<String> arguments = new List<string>();
                        if (arr.Length > 1)
                        {
                            for (int i = 1; i < arr.Length; i++)
                                arguments.Add(arr[i]);
                        }

                        String command = arr[0];

                        if (command.ToLower().Equals("exit"))
                        {
                            consoleThread.Abort();
                            callbackThread.Abort();
                            return;
                        }

                        if (command.StartsWith("@"))
                            Console.WriteLine(DoRequest(host, "evaluate", command));
                        else
                        {
                            String response = DoRequest(host, command, arguments.ToArray());
                            if (response.ToLower().Equals("resumed"))
                                suspended = false;
                            else
                                Console.WriteLine(response);
                        }
                    }
                    finally
                    {
                    }
                }
            }
        }

        private static void WaitConsoleData(object host)
        {
            try
            {
                var ub = new UriBuilder(String.Format(@"{0}:8081/console/attach", host.ToString()));
                var queryString = System.Web.HttpUtility.ParseQueryString(ub.Uri.Query, Encoding.UTF8);
                ub.Query = queryString.ToString();

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ub.Uri);
                req.Timeout = System.Threading.Timeout.Infinite;
                try
                {
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                    using (System.IO.StreamReader r = new System.IO.StreamReader(resp.GetResponseStream()))
                    {
                        while (true)
                        {
                            String line = r.ReadLine();
                            messages.Enqueue(line);
                            if (!suspended)
                            {
                                while (messages.Count > 0)
                                    Console.WriteLine(messages.Dequeue());
                            }
                        }
                    }
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static String DoRequest(String host, String command, params object[] arguments)
        {
            using (Stream s = DoRequestStream(host, command, arguments))
            {
                return new System.IO.StreamReader(s).ReadToEnd();
            }
        }

        private static Stream DoRequestStream(String host, String command, params object[] arguments)
        {
            var ub = new UriBuilder(String.Format(@"{0}:8082/debugger/{1}", host, command));
            var queryString = System.Web.HttpUtility.ParseQueryString(ub.Uri.Query, Encoding.UTF8);
            if (arguments != null)
            {
                int cnt = 1;
                foreach (object obj in arguments)
                {
                    queryString.Add(String.Format("param{0}", cnt), obj.ToString());
                    cnt++;
                }
            }
            ub.Query = queryString.ToString();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ub.Uri);
            req.Timeout = System.Threading.Timeout.Infinite;
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                return resp.GetResponseStream();
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

        private static void WaitCallback(object obj)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:8082" + locapath);
                listener.Start();

                Console.WriteLine("Start listen on port 8082");

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    try
                    {
                        using (System.IO.StreamWriter wr = new StreamWriter(response.OutputStream))
                        {
                            response.StatusCode = 200;
                            try
                            {
                                string method = request.Url.LocalPath.Remove(0, locapath.Length);
                                string query = request.Url.Query;

                                List<string> parameters = new List<string>();
                                if (!string.IsNullOrWhiteSpace(query))
                                {
                                    query = query.Remove(0, 1);
                                    foreach (string param in query.Split('&'))
                                    {
                                        string value = param.Split('=')[1];
                                        value = WebUtility.UrlDecode(value);
                                        parameters.Add(value);
                                    }
                                }

                                if (method.Equals("suspended"))
                                {
                                    suspended = true;
                                    Console.WriteLine("suspended at " + parameters[0]);
                                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parameters[1] + "==")));
                                }

                            }
                            catch (Exception e)
                            {
                            }
                        }
                        response.Close();
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        

    }
}
