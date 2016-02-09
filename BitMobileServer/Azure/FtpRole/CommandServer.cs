using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Collections.Specialized;

namespace FtpRole
{
    class CommandServer
    {
        private String address;
        private int port;
        private Dictionary<String, Func<NameValueCollection, String>> actions;

        public CommandServer(String address, int port, Dictionary<String, Func<NameValueCollection, String>> actions)
        {
            this.address = address;
            this.port = port;
            this.actions = actions;
        }

        public void Start()
        {
            new System.Threading.Thread(Execute).Start();
        }

        private void Execute()
        {
            HttpListener listener = new HttpListener();
            listener.IgnoreWriteExceptions = true;
            listener.Prefixes.Add(String.Format("http://{0}:{1}/", address, port.ToString()));
            listener.Start();
            Console.WriteLine(String.Format("listening port {0}...", port.ToString()));
            String path = "";

            bool terminated = false;
            while (!terminated)
            {
                Console.WriteLine("wait for request..");
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                try
                {
                    path = request.Url.LocalPath;
                    Console.WriteLine(path);

                    String action = path.Substring(1, path.Length - 1);

                    String result = "";
                    if (actions.ContainsKey(action))
                    {
                        result = actions[action](request.QueryString);
                    }

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
                    response.ContentLength64 = buffer.Length;

                    int blockSize = 1024;
                    int i = 0;
                    while (i < buffer.Length)
                    {
                        if (buffer.Length - i < blockSize)
                            blockSize = buffer.Length - i;
                        response.OutputStream.Write(buffer, i, blockSize);
                        i += blockSize;
                    }
                    response.OutputStream.Flush();
                    response.Close();
                }
                catch
                {
                    // todo
                }
            }
            listener.Stop();
        }
    }

}
