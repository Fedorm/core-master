using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace PushService
{
    [ServiceContract]
    interface IPushServiceRequestHandler
    {
        [WebInvoke(UriTemplate = "sendmessage", Method = "POST"), OperationContract]
        Stream SendMessage(Stream messageBody);

        [WebInvoke(UriTemplate = "android/project", Method = "GET"), OperationContract]
        Stream GetAndroidProjectId();

        [WebInvoke(UriTemplate = "register", Method = "GET"), OperationContract]
        Stream RegisterDevice();
    }
}
