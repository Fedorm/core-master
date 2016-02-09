using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace SystemService
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface ISystemRequestHandler
    {
        [WebInvoke(UriTemplate = "version", Method = "GET"), OperationContract]
        Stream Version();

        [WebInvoke(UriTemplate = "licenses", Method = "GET"), OperationContract]
        Stream Licenses();

        [WebInvoke(UriTemplate = "licenses/add", Method = "POST"), OperationContract]
        Stream AddLicense(Stream messageBody);

        [WebInvoke(UriTemplate = "solutions", Method = "GET"), OperationContract]
        Stream Solutions();

        [WebInvoke(UriTemplate = "solutions/remove", Method = "GET"), OperationContract]
        Stream RemoveSolutions();

        [WebInvoke(UriTemplate = "solutions/remove/{name}", Method = "GET"), OperationContract]
        Stream RemoveSolution(String name);

        [WebInvoke(UriTemplate = "solutions/create/{name}", Method = "GET"), OperationContract]
        Stream CreateSolution(String name);

        [WebInvoke(UriTemplate = "solutions/move/{fromName}/{toName}", Method = "GET"), OperationContract]
        Stream MoveSolution(String fromName, String toName);

        [WebInvoke(UriTemplate = "solutions/setpassword/{name}/{password}", Method = "GET"), OperationContract]
        Stream SetPassword(String name, String password);

        [WebInvoke(UriTemplate = "solutions/getpassword/{name}", Method = "GET"), OperationContract]
        Stream GetPassword(String name);

        [WebInvoke(UriTemplate = "solutions/setlicenses/{name}/{licenses}", Method = "GET"), OperationContract]
        Stream SetLicenses(String name, String licenses);
    }
}
