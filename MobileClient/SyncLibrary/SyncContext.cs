using System;
using System.Xml;
using BitMobile.Common.SyncLibrary;
using BitMobile.SyncLibrary.BitMobile;

namespace BitMobile.SyncLibrary
{
    public class SyncContext : ISyncContext
    {
        public IOfflineContext CreateOfflineContext(XmlDocument metadata, string cachePath, Uri uri)
        {
            return new OfflineContext(metadata, cachePath, uri);
        }
    }
}
