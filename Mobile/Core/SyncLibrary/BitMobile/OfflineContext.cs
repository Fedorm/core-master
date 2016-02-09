using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using BitMobile.Common.Entites;
using BitMobile.DbEngine;
using BitMobile.ValueStack;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;

namespace BitMobile.SyncLibrary.BitMobile
{
    public class OfflineContext : IsolatedStorageOfflineContext
    {
        public const string SyncScopeName = "DefaultScope";

        public OfflineContext(XmlDocument metadata, string cachePath, Uri uri)
            : base(GetSchema(metadata), SyncScopeName, cachePath, uri)
        {
            Constant = EntityFactory.GetConstants(metadata);
        }

        public IDictionary<string, object> Constant { get; private set; }

        static IsolatedStorageSchema GetSchema(XmlDocument metadata)
        {

            EntityType[] entities = EntityFactory.RegisterKnownTypes(metadata);
            return new IsolatedStorageSchema(entities);
        }
    }
}
