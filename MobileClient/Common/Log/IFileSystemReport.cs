using System;

namespace BitMobile.Common.Log
{
    public interface IFileSystemReport
    {
        int LoadedCount { get; set; }
        long LoadedSize { get; set; }
        int DeletedCount { get; set; }
        long DeletedSize { get; set; }
        void Send(Exception exception = null);
    }
}