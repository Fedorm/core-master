using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Collections.Generic;

using System.ServiceModel;
using System.ServiceModel.Activation;
using FileHelperForCloud;
using System.Security.Policy;
using System.ServiceModel.Channels;

namespace AdminService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AdminSyncService : IRequestHandler
    {
        private Common.Solution solution;

        public AdminSyncService(String scope, System.Net.NetworkCredential credential)
        {
            Common.Logon.CheckAdminCredential(scope, credential);
            this.solution = Common.Solution.CreateFromContext(scope);
        }

        private RequestHandlerProxy GetProxy()
        {
            //return Common.DomainManager.GetProxy<RequestHandlerProxy>(solution.Name, System.Reflection.Assembly.GetExecutingAssembly().FullName);
            return new RequestHandlerProxy();
        }

        public Stream UploadMetadata(Stream messageBody)
        {
            if (solution.IsAsured)
                throw new NotImplementedException();
            return GetProxy().UploadMetadata(solution, messageBody);
        }

        public Stream UploadMetadata2(Stream messageBody)
        {
            return GetProxy().UploadMetadata2(solution, messageBody);
        }

        public Stream UploadMetadataFiltersOnly(Stream messageBody)
        {
            return GetProxy().UploadMetadataFiltersOnly(solution, messageBody);
        }

        public Stream UploadMetadataAsync(Stream messageBody)
        {
            if(solution.IsAsured)
                throw new NotImplementedException();
            return GetProxy().UploadMetadataAsync(solution, messageBody);
        }

        public Stream UploadMetadata2Async(Stream messageBody)
        {
            return GetProxy().UploadMetadata2Async(solution, messageBody);
        }

        public Stream ReDeployMetadata()
        {
            return GetProxy().ReDeployMetadata(solution);
        }

        public Stream DeploySolutionPackage(Stream messageBody)
        {
            return GetProxy().DeploySolutionPackage(solution, messageBody);
        }

        public Stream UploadData(Stream messageBody)
        {
            throw new NotImplementedException();
            //return GetProxy().UploadData(solution, messageBody);
        }

        public Stream UploadData2(Stream messageBody)
        {
            return GetProxy().UploadData2(solution, messageBody);
        }

        public Stream UploadData2Async(Stream messageBody)
        {
            return GetProxy().UploadData2Async(solution, messageBody);
        }

        public Stream UploadData3(Stream messageBody)
        {
            return GetProxy().UploadData3(solution, messageBody);
        }

        public Stream UploadData3Async(Stream messageBody)
        {
            return GetProxy().UploadData3Async(solution, messageBody);
        }

        public Stream DeploySolution(Stream resourcesZipFileStream)
        {
            return GetProxy().DeploySolution(solution, resourcesZipFileStream);
        }

        public Stream UpdateResources()
        {
            return GetProxy().UpdateResources(solution);
        }

        public Stream DownloadData(Stream messageBody)
        {
            return GetProxy().DownloadData(solution, messageBody);
        }

        public Stream DownloadDataCommit(Stream messageBody)
        {
            return GetProxy().DownloadDataCommit(solution, messageBody);
        }

        public Stream DownloadDeleted()
        {
            return GetProxy().DownloadDeleted(solution);
        }

        public Stream DownloadDeviceLog()
        {
            return GetProxy().DownloadDeviceLog(solution);
        }

        public Stream CheckIfExists(Stream messageBody)
        {
            return GetProxy().CheckIfExists(solution, messageBody);
        }

        public Stream GetAllStorageData()
        {
            return GetProxy().GetAllStorageData(solution);
        }

        public Stream GetClientDll()
        {
            return Invoke(GetProxy().GetClientDll);
        }

        public Stream GetClientMetadata()
        {
            return Invoke(GetProxy().GetClientMetadata);
        }

        public Stream ArchiveFileSystem()
        {
            return Invoke(GetProxy().ArchiveFileSystem);
        }

        public Stream AsyncTaskStatus(String id)
        {
            return GetProxy().AsyncTaskStatus(solution, id);
        }

        private Stream Invoke(Func<Common.Solution, Stream> func)
        {
            return func(solution);
        }

        private Stream Invoke(Stream messageBody, Func<Common.Solution, Stream, Stream> func)
        {
            return func(solution, messageBody);
        }

    }

}
