using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace GPSService
{
    [ServiceContract]
    interface IGPSRequestHandler
    {
        [WebInvoke(UriTemplate = "PostData", Method = "POST"), OperationContract]
        Stream PostData(Stream messageBody);
    }
}
