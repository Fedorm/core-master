using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BitMobile.Debugger
{
    public class DebugConsole
    {
        static readonly object LockObject = new object();
        const string Locapath = "/console/";

        private static DebugConsole _console;
        public static DebugConsole Console
        {
            get { return _console ?? (_console = new DebugConsole()); }
        }

        public static void Init()
        {
            Console.Start();
        }

        readonly List<String> _messages;
        readonly Semaphore _semaphore = new Semaphore(0, 1000);
        bool _attached;

        private DebugConsole()
        {
            _messages = new List<string>();
        }

        public void WriteLine(String s)
        {
            if (_attached)
            {
                lock (LockObject)
                {
                    _messages.Add(s);
                    _semaphore.Release(1);
                }
            }
        }

        private void Start()
        {
            var thread = new Thread(WaitCommand);
            thread.IsBackground = true;
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            try
            {
                var listener = new TcpListener(8081);
                listener.Start();
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream s = client.GetStream();

                _attached = true;
                WriteLine("Welcome to BitMobile device console !");
                while (true)
                {
                    try
                    {
                        using (var wr = new StreamWriter(s))
                        {
                            try
                            {
                                while (true)
                                {
                                    _semaphore.WaitOne();
                                    String msg;
                                    lock (LockObject)
                                    {
                                        msg = _messages[0];
                                        _messages.RemoveAt(0);
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
// ReSharper disable EmptyGeneralCatchClause
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause

        }

    }
}
