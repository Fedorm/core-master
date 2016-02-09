using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BitMobile.ClientModel
{
    public class HttpRequest
    {
        public String Host { get; set; }

        public HttpRequest()
        {
        }

        public HttpRequest(String host)
        {
            this.Host = host;
        }

        public String Get(String query)
        {
            String result;
            var ub = new UriBuilder(String.Format(@"{0}/{1}", Host, query));
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

        public String Post(String query, String data)
        {
            String result;
            var ub = new UriBuilder(String.Format(@"{0}/{1}", Host, query));
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ub.Uri);
            req.Method = "POST";
            try
            {
                System.IO.Stream s = req.GetRequestStream();
                using (System.IO.StreamWriter w = new StreamWriter(s, System.Text.Encoding.UTF8))
                {
                    w.Write(data);
                    w.Flush();
                }

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
    }
}
