using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace ImageService
{
    [ServiceContract]
    interface IImageRequestHandler
    {
        [WebInvoke(UriTemplate = "{catalog}/put/{id}", Method = "POST"), OperationContract]
        Stream UploadImage(Stream messageBody, String catalog, String id);

        [WebInvoke(UriTemplate = "{catalog}/get/{id}", Method = "GET"), OperationContract]
        Stream GetImage(String catalog, String id);

        [WebInvoke(UriTemplate = "{catalog}/delete/{id}", Method = "GET"), OperationContract]
        Stream DeleteImage(String catalog, String id);

        [WebInvoke(UriTemplate = "{catalog}/list", Method = "GET"), OperationContract]
        Stream List(String catalog);

    }
}
