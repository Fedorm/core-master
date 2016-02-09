using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace LicenseService
{
    [ServiceContract]
    public interface ILicenseRequestHandler
    {
        [WebInvoke(UriTemplate = "initservice", Method = "GET"), OperationContract]
        Stream InitService();

        [WebInvoke(UriTemplate = "createlicense", Method = "GET"), OperationContract]
        Stream CreateLicense();

        [WebInvoke(UriTemplate = "activatelicense", Method = "POST"), OperationContract]
        Stream ActivateLicense(Stream messageBody);
    }
}
