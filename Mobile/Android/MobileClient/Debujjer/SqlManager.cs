using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace BitMobile.Debugger
{
    public class SqlManager : BitMobile.DbEngine.IDatabaseAware
    {
        private static String localpath = "/";
        private static SqlManager manager = null;

        private BitMobile.DbEngine.IDatabase database;

        public static SqlManager CreateInstance()
        {
            if (manager == null)
            {
                manager = new SqlManager();
                manager.Start();
            }
            return manager;
        }

        private SqlManager()
        {
        }

        public void SetDatabase(DbEngine.IDatabase database)
        {
            this.database = database;
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
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:8083" + localpath);
                listener.Start();

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
                                string method = request.Url.LocalPath.Remove(0, localpath.Length);
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

                                method = String.IsNullOrEmpty(method) ? "query" : method;
                                switch (method.ToLower())
                                {
                                    case "query":
                                        DoQuery(parameters.ToArray(), wr);
                                        break;
                                    case "result":
                                        DoResult(parameters.ToArray(), wr);
                                        break;
                                    case "xmlresult":
                                        DoXmlResult(parameters.ToArray(), wr);
                                        break;
                                    default:
                                        WriteHtml("Unknown command", wr);
                                        break;
                                }
                                wr.Flush();

                            }
                            catch (Exception e)
                            {
                                WriteHtml(e.Message, wr);
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

        public void DoQuery(String[] parameters, System.IO.StreamWriter w)
        {
            w.WriteLine("<html>");
            w.WriteLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'");
            w.WriteLine("<body>");
            w.WriteLine("<form action='result' method='get'>");
            w.WriteLine("<p><b>Введите sql запрос:</b></p>");
            w.WriteLine("<p><textarea rows='10' cols='45' name='query'></textarea></p>");
            w.WriteLine("<p><input type='submit'></p>");
            w.WriteLine("</form");
            w.WriteLine("</body>");
            w.WriteLine("</html>");
        }

        public void DoXmlResult(String[] parameters, System.IO.StreamWriter w)
        {
            String sql = parameters[0];

            System.Data.DataTable tbl = database.SelectAsDataTable("query", sql, new object[] { });
            tbl.WriteXml(w);
        }

        public void DoResult(String[] parameters, System.IO.StreamWriter w)
        {
            String sql = parameters[0];

            System.Data.DataTable tbl = database.SelectAsDataTable("query", sql, new object[] { });

            w.WriteLine("<html>");
            w.WriteLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'");
            w.WriteLine("<body>");
            w.WriteLine("<table width='100%' border='0' cellspacing='0' cellpadding='0'>");

            w.WriteLine("<tr>");
            foreach (System.Data.DataColumn c in tbl.Columns)
            {
                w.WriteLine("<td style='border-bottom: 1px solid #aaa;border-right: 1px solid #aaa;'><b>" + c.Caption + "</b></td>");
            }
            w.WriteLine("</tr>");

            foreach (System.Data.DataRow r in tbl.Rows)
            {
                w.WriteLine("<tr>");
                for (int i = 0; i < tbl.Columns.Count; i++)
                {
                    w.WriteLine("<td style='border-bottom: 1px solid #aaa;border-right: 1px solid #aaa;'>" + r[i] + "</td>");
                }
                w.WriteLine("</tr>");
            }

            w.WriteLine("</body>");
            w.WriteLine("</html>");
        }

        public void WriteHtml(String text, System.IO.StreamWriter w)
        {
            w.WriteLine("<html>");
            w.WriteLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'");
            w.WriteLine("<body>");
            w.WriteLine(text);
            w.WriteLine("</body>");
            w.WriteLine("</html>");
        }

    }
}
