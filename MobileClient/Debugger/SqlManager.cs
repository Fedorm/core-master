using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using BitMobile.Common.DbEngine;

namespace BitMobile.Debugger
{
    public class SqlManager : IDatabaseAware
    {
        private const String Localpath = "/";
        private static SqlManager _manager;
        private IDatabase _database;

        public static SqlManager CreateInstance()
        {
            if (_manager == null)
            {
                _manager = new SqlManager();
                _manager.Start();
            }
            return _manager;
        }

        private SqlManager()
        {
        }

        public void SetDatabase(IDatabase database)
        {
            _database = database;
        }

        public void Start()
        {
            var thread = new Thread(WaitCommand);
            thread.IsBackground = true;
            thread.Start();
        }

        void WaitCommand(object obj)
        {
            try
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://+:8083" + Localpath);
                listener.Start();

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    try
                    {
                        using (var wr = new StreamWriter(response.OutputStream))
                        {
                            response.StatusCode = 200;
                            try
                            {
                                string method = request.Url.LocalPath.Remove(0, Localpath.Length);
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
                                    case "database":
                                        DoDatabase(request, wr);
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
                        // ReSharper disable EmptyGeneralCatchClause
                        catch { }
                    }
                }
            }
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        public void DoDatabase(HttpListenerRequest request, StreamWriter w)
        {
            if (!request.HasEntityBody)
            {
                w.WriteLine("No file selected");
                return;
            }

            byte[] file = null;
            using (var body = request.InputStream)
            {
                using (var ms = new MemoryStream())
                {
                    body.CopyTo(ms);
                    file = ms.ToArray();
                }
            }

            if (file.Length == 0)
            {
                w.WriteLine("Empty file");
                return;
            }

            // skip form header
            var skipped = file.AsEnumerable();
            for (int i = 0; i < 4; i++)
            {
                skipped=skipped.SkipWhile(c => c != '\n').Skip(1);
            }
            file = skipped.ToArray();

            // and skip footer
            for (int i = file.Length-5; i > 0; i--)
            {
                if (file[i] == '\r' && file[i+1] == '\n' && file[i+2] == '-' && file[i+3] == '-')
                {
                    var arr = new byte[i];
                    Array.Copy(file,arr,i);
                    file = arr;
                }
            }

            if (Application.DbEngine.DbContext.Current.Database == null)
            {
                w.WriteLine("DB not initialized");
                return;
            }

            Application.DbEngine.DbContext.Current.ReplaceDatabase(file);

            w.WriteLine("Ok");
        }


        public void DoQuery(String[] parameters, StreamWriter w)
        {
            w.WriteLine("<html>");
            w.WriteLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'");
            w.WriteLine("<body>");
            w.WriteLine("<form action='result' method='get'>");
            w.WriteLine("<p><b>Введите sql запрос:</b></p>");
            w.WriteLine("<p><textarea rows='10' cols='45' name='query'></textarea></p>");
            w.WriteLine("<p><input type='submit'></p>");
            w.WriteLine("</form>");
            w.WriteLine("<br/><br/>");
            w.WriteLine("<form action='database' method='post' enctype='multipart/form-data'>");
            w.WriteLine("<p><b>Загрузить SQLite базу</b></p>");
            w.WriteLine("<p><input type='file' name='db' size='6    0'></p>");
            w.WriteLine("<p><input type='submit'></p>");
            w.WriteLine("</form>");
            w.WriteLine("</body>");
            w.WriteLine("</html>");
        }

        public void DoXmlResult(String[] parameters, StreamWriter w)
        {
            String sql = parameters[0];

            System.Data.DataTable tbl = _database.SelectAsDataTable("query", sql, new object[] { });
            tbl.WriteXml(w);
        }

        public void DoResult(String[] parameters, StreamWriter w)
        {
            String sql = parameters[0];

            System.Data.DataTable tbl = _database.SelectAsDataTable("query", sql, new object[] { });

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

        public void WriteHtml(String text, StreamWriter w)
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
