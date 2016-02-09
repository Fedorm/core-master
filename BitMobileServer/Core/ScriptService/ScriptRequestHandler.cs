using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Security.Policy;
using System.ServiceModel.Web;

using BitMobile.ValueStack;
using ScriptService.Model;
using ScriptService.Persist;
using Telegram;

namespace ScriptService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ScriptService : IScriptRequestHandler
    {
        private Common.Solution solution;

        public ScriptService(String scope, System.Net.NetworkCredential credential)
        {
            //Common.Logon.CheckAdminCredential(scope, credential);
            this.solution = Common.Solution.CreateFromContext(scope);
        }

        public Stream GetStyle(String name)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/css";
            String modulePath = String.Format(@"{0}\resource\server\css\{1}", solution.SolutionFolder, name);
            if (System.IO.File.Exists(modulePath))
            {
                System.IO.MemoryStream ms = new MemoryStream();
                using (System.IO.FileStream f = System.IO.File.OpenRead(modulePath))
                {
                    f.CopyTo(ms);
                }
                ms.Position = 0;
                return ms;
            }
            else
                return null;
        }


        public Stream GetImage(String name)
        {
            String modulePath = String.Format(@"{0}\resource\server\image\{1}", solution.SolutionFolder, name);
            if (System.IO.File.Exists(modulePath))
            {
                switch (new System.IO.FileInfo(modulePath).Extension.ToLower())
                {
                    case "jpg":
                    case "jpeg":
                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                        break;
                    case "png":
                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/png";
                        break;
                }

                System.IO.MemoryStream ms = new MemoryStream();
                using (System.IO.FileStream f = System.IO.File.OpenRead(modulePath))
                {
                    f.CopyTo(ms);
                }
                ms.Position = 0;
                return ms;
            }
            else
                return null;
        }

        public Stream CallFunction1g()
        {
            return CallFunction("main", "main", null);
        }

        public Stream CallFunction1p(Stream messageBody)
        {
            return CallFunction("main", "main", messageBody);
        }

        public Stream CallFunction2g(String module)
        {
            return CallFunction(module, "main", null);
        }

        public Stream CallFunction2p(String module, Stream messageBody)
        {
            return CallFunction(module, "main", messageBody);
        }

        public Stream CallFunction3g(String module, String function)
        {
            return CallFunction(module, function, null);
        }

        public Stream CallFunction3p(String module, String function, Stream messageBody)
        {
            return CallFunction(module, function, messageBody);
        }

        private Stream CallFunction(String module, String function, Stream messageBody)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            ValueStack stack = new ValueStack();
            AddParameters(stack, messageBody);
                
            try
            {
                String modulePath = String.Format(@"{0}\resource\server\script\{1}.js", solution.SolutionFolder, module);
                if (System.IO.File.Exists(modulePath))
                {
                    System.IO.FileInfo fi = new FileInfo(modulePath);

                    using (System.IO.Stream scriptStream = System.IO.File.OpenRead(modulePath))
                    {
                        BitMobile.Script.ScriptEngine engine = BitMobile.Script.ScriptEngine.LoadScript(scriptStream, module, fi.LastWriteTimeUtc);
                        BitMobile.Script.ScriptEngine.RegisterType("DateTime", typeof(DateTime));
                        BitMobile.Script.ScriptEngine.RegisterType("Guid", typeof(Guid));

                        engine.AddVariable("Guid", typeof(Guid));
                        engine.AddVariable("DateTime", typeof(DateTime));

                        var db = new DB(solution.ConnectionString);
                        engine.AddVariable("DB", db);
                        engine.AddVariable("Tracker", new Tracker());
                        engine.AddVariable("$", stack);
                        engine.AddVariable("View", new BitMobile.MVC.ViewFactory(String.Format(@"{0}\resource\server\view", solution.SolutionFolder), stack));
                        engine.AddVariable("Telegram", new TelegramFactory(new TelegramSettignsPersist(solution.ConnectionString)));
                        

                        object result = engine.CallFunction(function);

                        if (result == null)
                            return Common.Utils.MakeTextAnswer("ok");
                        else
                        {
                            if (result is BitMobile.MVC.BaseView)
                            {
                                BitMobile.MVC.BaseView view = (BitMobile.MVC.BaseView)result;
                                WebOperationContext.Current.OutgoingResponse.ContentType = view.ContentType();
                                return view.Translate();
                            }

                            else
                                return Common.Utils.MakeTextAnswer(result.ToString());
                        }
                    }
                }
                else
                    return Common.Utils.MakeTextAnswer("Invalid module name");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        private void AddParameters(ValueStack stack, Stream messageBody)
        {
            var ps = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            for (int i = 0; i < ps.Count; i++)
            {
                object v = ps.GetValues(i);
                if (v.GetType() == typeof(String[]))
                    v = ((String[])v)[0].ToString();

                stack.Push(ps.GetKey(i), v);
            }

            if (messageBody != null)
            {
                using (System.IO.StreamReader r = new StreamReader(messageBody, System.Text.Encoding.UTF8, false, 1024, true))
                {
                    while (!r.EndOfStream)
                    {
                        string line = r.ReadLine();
                        if(!String.IsNullOrEmpty(line))
                        {
                            string[] arr = line.Split('=');
                            if (arr.Length == 2)
                            {
                                String v = System.Net.WebUtility.UrlDecode(arr[1].Trim());
                                stack.Push(arr[0].Trim(), v);
                            }
                        }
                    }
                }
            }
        }
    }

}
