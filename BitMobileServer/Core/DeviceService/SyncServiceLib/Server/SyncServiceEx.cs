using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Xml;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Services.SqlProvider;
using System.Security.Permissions;
using System.ServiceModel.Web;
using System.Linq;

namespace Microsoft.Synchronization.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall),
        AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SyncServiceEx : IRequestHandler
    {
        private Common.Solution solution;
        private NetworkCredential credential;

        public SyncServiceEx(String scope, NetworkCredential credential)
        {
            this.solution = Common.Solution.CreateFromContext(scope);
            this.credential = credential;
        }

        private object GetProxy()
        {
            String[] args = new String[] { solution.DatabaseServer, solution.RootFolder, solution.BitMobileServerId, solution.IsAsured.ToString(), solution.RootPassword };
            object obj = Common.DomainManager.GetProxy(solution.Name, solution.Name, "DefaultScope.DeviceSyncService", new AppDomainInitializer(AppDomainInit), args);
            return obj;
        }

        public static void AppDomainInit(string[] args)
        {
            Common.Solution.InitContext(
                args[0],
                args[1],
                args[2],
                bool.Parse(args[3]),
                args[4]
            );

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Equals(AppDomain.CurrentDomain.FriendlyName))
            {
                Common.Solution solution = Common.Solution.CreateFromContext(AppDomain.CurrentDomain.FriendlyName);

                String[] arr = System.IO.Directory.GetFiles(System.IO.Path.Combine(solution.SolutionFolder, "bin"), "Server*.dll", System.IO.SearchOption.TopDirectoryOnly);
                if (arr.Length == 0)
                    throw new Exception("No Server.dll found.");
                Array.Sort(arr);
                String fileName = arr[arr.Length - 1];

                return System.Reflection.Assembly.LoadFile(fileName);
            }
            else
                return null;
        }

        private HttpContextServiceHost CreateContext(Stream messageBody)
        {
            HttpContextServiceHost ctx = new HttpContextServiceHost(messageBody);

            ctx.UserName = credential.UserName;
            ctx.UserPassword = credential.Password;
            ctx.ContentType = WebOperationContext.Current.IncomingRequest.ContentType;
            ctx.HostHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host];
            ctx.QueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            ctx.RequestUri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
            ctx.RelativeUriSegments = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RelativePathSegments.ToArray();
            ctx.RequestAccept = WebOperationContext.Current.IncomingRequest.Accept;
            ctx.RequestHeaders = WebOperationContext.Current.IncomingRequest.Headers;
            ctx.RequestHttpMethod = WebOperationContext.Current.IncomingRequest.Method;
            ctx.RequestIfMatch = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.IfMatch];
            ctx.RequestIfNoneMatch = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.IfNoneMatch];
            ctx.ServiceBaseUri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri;

            HttpRequestMessageProperty g = OperationContext.Current.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            ctx.Headers = g.Headers;

            return ctx;
        }

        public Stream ProcessRequestForMessage(Stream messageBody)
        {
            HttpContextServiceHost ctx = CreateContext(messageBody);
            Stream result;

            if (!Common.Logon.CheckServerLicence(ctx.Headers["deviceId"]))
                result = MessageToStream(ctx, CreateLicenseExceptionMessage(ctx, "Limit of server licenses exceeded"));
            else
                result = ((IRequestHandlerProxy)GetProxy()).ProcessRequest(ctx);

            WebOperationContext.Current.OutgoingResponse.Headers[SyncServiceConstants.SYNC_SERVICE_USERID] = ctx.UserId;
            WebOperationContext.Current.OutgoingResponse.Headers[SyncServiceConstants.SYNC_SERVICE_USEREMAIL] = (String.IsNullOrEmpty(ctx.UserEMail) ? "" : ctx.UserEMail);
            WebOperationContext.Current.OutgoingResponse.Headers[SyncServiceConstants.SYNC_SERVICE_CONTENTLENGTH] = ctx.OriginalContentLength.ToString();
            WebOperationContext.Current.OutgoingResponse.Headers[SyncServiceConstants.SYNC_SERVICE_RESOURCEVERSION] = ctx.ResourceVersion == null ? "" : ctx.ResourceVersion;
            WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            WebOperationContext.Current.OutgoingResponse.StatusCode = ctx.StatusCode;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = ctx.StatusDescription;
            return result;
        }

        public Stream GetClientDll()
        {
            try
            {
                string pathToClientDll = Path.Combine(solution.SolutionFolder, @"bin\Client.dll");

                FileStream fs = File.OpenRead(pathToClientDll);
                return fs;
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        public Stream GetClientMetadata()
        {

            Common.Logon.CheckUserCredential2(solution.Name, credential);

            try
            {
                
                string pathToClientDll = Path.Combine(solution.SolutionFolder, @"configuration\configuration.xml");

                FileStream fs = File.OpenRead(pathToClientDll);
                return fs;
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        public Stream Ping()
        {
            return Common.Utils.MakeTextAnswer("pong");
        }

        public Stream LogWebDav(Stream messageBody)
        {
            var doc = new XmlDocument();
            doc.Load(messageBody);

            var content = new Dictionary<string, object>();
            if (doc.DocumentElement != null)
            {
                foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
                {
                    string text = childNode.InnerText;

                    object value;
                    switch (childNode.Name)
                    {
                        case "UserId":
                            value = Guid.Parse(text);
                            break;
                        case "StartTime":
                        case "EndTime":
                            value = DateTime.Parse(text);
                            break;
                        case "State":
                            value = bool.Parse(text);
                            break;
                        case "LoadedSize":
                        case "LoadedCount":
                        case "DeletedSize":
                        case "DeletedCount":
                            value = int.Parse(text);
                            break;
                        case "Error":
                        case "Directory":
                        case "ResourceVersion":
                        case "CoreVersion":
                        case "ConfigName":
                        case "ConfigVersion":
                        case "DeviceId":
                            value = text;
                            break;
                        default:
                            // unknown
                            throw new IndexOutOfRangeException(childNode.Name);
                    }
                    content.Add(childNode.Name.ToLower(), value);
                }
            }

            Common.Logon.WriteWebDavLog(solution.Name, content);
            return Common.Utils.MakeTextAnswer("ok");
        }

        /*
        protected System.IO.Stream MakeExceptionAnswer(Exception e)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            text = text + e.StackTrace;
            return MakeTextAnswer(text);
        }
        
        private System.IO.Stream MakeTextAnswer(String text)
        {
            MemoryStream ms = new MemoryStream();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return ms;
        }
        */

        private Message CreateLicenseExceptionMessage(HttpContextServiceHost ctx, string errorDescription)
        {
            ctx.StatusCode = HttpStatusCode.PaymentRequired;
            ctx.StatusDescription = errorDescription;

            var error = new ServiceError { ErrorDescription = errorDescription };

            Message message;
            message = Message.CreateMessage(MessageVersion.None, String.Empty, error, new DataContractSerializer(typeof(ServiceError)));
            message.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Xml));

            var property = new HttpResponseMessageProperty { StatusCode = ctx.StatusCode };
            property.Headers.Add(HttpResponseHeader.ContentType, "application/atom+xml");

            message.Properties.Add(HttpResponseMessageProperty.Name, property);

            return message;
        }

        private System.IO.Stream MessageToStream(HttpContextServiceHost ctx, Message message)
        {
            System.IO.Stream ms = new System.IO.MemoryStream();
            using (System.IO.Stream tempms = new System.IO.MemoryStream())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(tempms))
                {
                    message.WriteMessage(writer);
                    writer.Flush();
                }
                ctx.OriginalContentLength = (int)tempms.Length;
                tempms.Position = 0;

                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                {
                    tempms.CopyTo(gzip);
                }
            }
            ms.Position = 0;

            return ms;
        }
    }
}
