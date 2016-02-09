using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BitMobile.Debugger
{
    public class DebugConsole
    {
        static object lockObject = new object();
        const string locapath = "/console/";

        public static DebugConsole console = null;
        public static DebugConsole Console
        {
            get
            {
                if (console == null)
                    console = new DebugConsole();
                return console;
            }
        }

        public static void Init()
        {
            Console.Start();
        }

        List<String> messages;
        Semaphore semaphore = new Semaphore(0, 1000);
        bool attached = false;

        public DebugConsole()
        {
            messages = new List<string>();
        }

        public void WriteLine(String s)
        {
            if (attached)
            {
                lock (lockObject)
                {
                    messages.Add(s);
                    semaphore.Release(1);
                }
            }
        }

        public void Start()
        {
            Thread thread = new Thread(WaitCommand);
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            try
            {
                TcpListener listener = new TcpListener(8081);
                listener.Start();
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream s = client.GetStream();

                attached = true;
                WriteLine("Welcome to BitMobile device console !");
                while (true)
                {
                    try
                    {
                        using (System.IO.StreamWriter wr = new StreamWriter(s))
                        {
                            try
                            {
                                while (true)
                                {
                                    semaphore.WaitOne();
                                    String msg;
                                    lock (lockObject)
                                    {
                                        msg = messages[0];
                                        messages.RemoveAt(0);
                                    }
                                    wr.WriteLine(msg);
                                    wr.Flush();
                                }
                            }
                            catch (Exception e)
                            {
                                wr.WriteLine(e.Message);
                            }
                        }
                    }
                    catch
                    {
                        try
                        {
                            client.Close();
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

/*
        void WaitCommand(object obj)
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:8081" + locapath);
                listener.Start();

                while (true)
                {
                    attached = false;
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    attached = true;
                    WriteLine("Welcome to BitMobile device console !");

                    try
                    {
                        using (System.IO.StreamWriter wr = new StreamWriter(response.OutputStream))
                        {
                            response.StatusCode = 200;
                            try
                            {
                                string method = request.Url.LocalPath.Remove(0, locapath.Length);
                                if (method.ToLower().Equals("attach"))
                                {
                                    while (true)
                                    {
                                        semaphore.WaitOne();
                                        String msg;
                                        lock (lockObject)
                                        {
                                            msg = messages[0];
                                            messages.RemoveAt(0);
                                        }
                                        wr.WriteLine(msg);
                                        wr.Flush();
                                    }
                                }
                                else
                                    throw new InvalidOperationException();
                            }
                            catch (Exception e)
                            {
                                wr.WriteLine(e.Message);
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
            catch
            {
            }
        }
*/

    }
}
