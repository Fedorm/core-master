using System;
using System.Xml;

namespace BitMobile.Common.SyncLibrary
{
    public interface ISyncContext
    {
        IOfflineContext CreateOfflineContext(XmlDocument metadata, string cachePath, Uri uri);
    }
}