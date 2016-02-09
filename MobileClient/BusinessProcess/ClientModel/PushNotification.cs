using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitMobile.Common.ScriptEngine;
using BitMobile.Common.Application;
using System.Collections;
using System.Xml;
using System.Net;
using BitMobile.Common.ValueStack;
using System.IO;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.Utils;

namespace BitMobile.BusinessProcess.ClientModel
{
    class PushNotification
    {
        private readonly IApplicationContext _context;
        private readonly IScriptEngine _scriptEngine;

        public PushNotification(IScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }
        
        public bool SendMessage(string data, object recipients)
        {
            try
            {
                String uri = String.Format("{0}/{1}", _context.Settings.BaseUrl, "push/sendmessage");
                var req = (HttpWebRequest)System.Net.WebRequest.Create(uri.ToCurrentScheme(_context.Settings.HttpsDisabled));
                req.Timeout = 15000; //15 sec
                req.Method = "POST";
                var commonData = (ICommonData)_context.ValueStack.Values["common"];
                string svcCredentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(commonData.UserId + ":" + _context.Settings.Password));
                req.Headers.Add("Authorization", "Basic " + svcCredentials);

                using (Stream requestStream = req.GetRequestStream())
                {
                    CreateMessage(data, recipients).Save(requestStream);
                }

                using (WebResponse response = req.GetResponse())
                {
                    using (Stream edata = response.GetResponseStream())
                    using (var reader = new StreamReader(edata))
                    {
                        String result = reader.ReadToEnd();
                        return result.Equals("ok");
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private XmlDocument CreateMessage(string data, object recipients)
        {
            var doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement e0 = doc.CreateElement(string.Empty, "Message", string.Empty);
            doc.AppendChild(e0);

            XmlElement e2 = doc.CreateElement(string.Empty, "Data", string.Empty);
            e2.AppendChild(doc.CreateTextNode(data));
            e0.AppendChild(e2);

            XmlElement e3 = doc.CreateElement(string.Empty, "Recipients", string.Empty);
            e0.AppendChild(e3);

            foreach (string s in ObjectToStringArray(recipients))
            {
                XmlElement e4 = doc.CreateElement(string.Empty, "Recipient", string.Empty);
                e4.AppendChild(doc.CreateTextNode(s));
                e3.AppendChild(e4);
            }

            return doc;
        }

        private IEnumerable<string> ObjectToStringArray(object obj)
        {
            if (obj == null)
                return new string[0];

            var str = obj as string;
            if (str != null)
                return new[] { str };

            var arr = obj as ArrayList;
            if (arr != null)
            {
                var result = new List<string>(arr.Count);
                result.AddRange(arr.OfType<string>());
                return result.ToArray();
            }

            throw new NonFatalException(D.INVALID_ARGUMENT_VALUE);
        }
    }
}