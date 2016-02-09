using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.IO;

namespace AdminService
{
    [ServiceContract]
    public interface IRequestHandler
    {
        [WebInvoke(UriTemplate = "UploadMetadata", Method = "POST"), OperationContract]
        Stream UploadMetadata(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadMetadata2", Method = "POST"), OperationContract]
        Stream UploadMetadata2(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadMetadataFiltersOnly", Method = "POST"), OperationContract]
        Stream UploadMetadataFiltersOnly(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadMetadataAsync", Method = "POST"), OperationContract]
        Stream UploadMetadataAsync(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadMetadata2Async", Method = "POST"), OperationContract]
        Stream UploadMetadata2Async(Stream messageBody);

        [WebInvoke(UriTemplate = "ReDeployMetadata", Method = "GET"), OperationContract]
        Stream ReDeployMetadata();

        [WebInvoke(UriTemplate = "DeploySolutionPackage", Method = "POST"), OperationContract]
        Stream DeploySolutionPackage(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadData", Method = "POST"), OperationContract]
        Stream UploadData(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadData2", Method = "POST"), OperationContract]
        Stream UploadData2(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadData3", Method = "POST"), OperationContract]
        Stream UploadData3(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadData2Async", Method = "POST"), OperationContract]
        Stream UploadData2Async(Stream messageBody);

        [WebInvoke(UriTemplate = "UploadData3Async", Method = "POST"), OperationContract]
        Stream UploadData3Async(Stream messageBody);

        [WebInvoke(UriTemplate = "DownloadData", Method = "POST"), OperationContract]
        Stream DownloadData(Stream messageBody);

        [WebInvoke(UriTemplate = "DownloadDataCommit", Method = "POST"), OperationContract]
        Stream DownloadDataCommit(Stream messageBody);

        [WebInvoke(UriTemplate = "DownloadDeleted", Method = "GET"), OperationContract]
        Stream DownloadDeleted();

        [WebInvoke(UriTemplate = "DownloadDeviceLog", Method = "GET"), OperationContract]
        Stream DownloadDeviceLog();

        [WebInvoke(UriTemplate = "CheckIfExists", Method = "POST"), OperationContract]
        Stream CheckIfExists(Stream messageBody);

        [WebInvoke(UriTemplate = "UpdateResources", Method = "GET"), OperationContract]
        Stream UpdateResources();

        [WebInvoke(UriTemplate = "GetAllStorageData", Method = "GET"), OperationContract]
        Stream GetAllStorageData();

        [WebInvoke(UriTemplate = "DeploySolution", Method = "POST"), OperationContract]
        Stream DeploySolution(Stream resourcesZipFileStream);

        [WebInvoke(UriTemplate = "GetClientDll", Method = "GET"), OperationContract]
        Stream GetClientDll();

        [WebInvoke(UriTemplate = "GetClientMetadata", Method = "GET"), OperationContract]
        Stream GetClientMetadata();

        [WebInvoke(UriTemplate = "ArchiveFileSystem", Method = "GET"), OperationContract]
        Stream ArchiveFileSystem();

        [WebInvoke(UriTemplate = "AsyncTaskStatus/{id}", Method = "GET"), OperationContract]
        Stream AsyncTaskStatus(String id);
    }
}
