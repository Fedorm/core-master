using System;
using System.Collections.Generic;
using BitMobile.Common.SyncLibrary;

namespace BitMobile.Common.DataAccessLayer
{
    public interface IDalContext
    {
        IDal CreateDal(IOfflineContext context
            , string appName
            , string language
            , string userName
            , string userPassword
            , string configName
            , string configVersion
            , IDictionary<string, string> deviceInfo
            , Action<int, int> progress);

        void Ping(string serviceUrl);
    }
}
