using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.IO;

namespace ScriptService
{
    [ServiceContract]
    public interface IScriptRequestHandler
    {
        [WebInvoke(UriTemplate = "css/{name}", Method = "GET"), OperationContract]
        Stream GetStyle(String name);

        [WebInvoke(UriTemplate = "image/{name}", Method = "GET"), OperationContract]
        Stream GetImage(String name);

        [WebInvoke(UriTemplate = "", Method = "GET"), OperationContract]
        Stream CallFunction1g();

        [WebInvoke(UriTemplate = "", Method = "POST"), OperationContract]
        Stream CallFunction1p(Stream messageBody);

        [WebInvoke(UriTemplate = "{module}", Method = "GET"), OperationContract]
        Stream CallFunction2g(String module);

        [WebInvoke(UriTemplate = "{module}", Method = "POST"), OperationContract]
        Stream CallFunction2p(String module, Stream messageBody);
         
        [WebInvoke(UriTemplate = "{module}/{function}", Method = "GET"), OperationContract]
        Stream CallFunction3g(String module, String function);

        [WebInvoke(UriTemplate = "{module}/{function}", Method = "POST"), OperationContract]
        Stream CallFunction3p(String module, String function, Stream messageBody);
    }
}
