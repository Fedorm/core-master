using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    class RemoteProxy
    {
        string _address;
        Console _console;

        public RemoteProxy(String address, Console console)
        {
            _address = address;
            _console = console;
        }

        protected string DoRequestString(String command, params object[] arguments)
        {
            using (Stream stream = DoRequestStream(command, arguments))
            using (StreamReader readStream = new StreamReader(stream, Encoding.UTF8))
                return readStream.ReadToEnd();
        }

        protected Stream DoRequestStream(String command, params object[] arguments)
        {
            Thread.Sleep(_console.CommandPause);

            var ub = new UriBuilder(String.Format(@"{0}/testserver/{1}", _address, command));
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
    }
}
