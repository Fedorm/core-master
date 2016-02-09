using System;
using System.Collections.Generic;
using System.Net;
using BitMobile.Application;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.SyncLibrary;
using BitMobile.Common.Utils;

namespace BitMobile.DataAccessLayer
{
    public class DalContext: IDalContext
    {
        public IDal CreateDal(IOfflineContext context, string appName, string language, string userName, string userPassword,
            string configName, string configVersion, IDictionary<string, string> deviceInfo, Action<int, int> progress)
        {
            return new Dal(context, appName, language, userName, userPassword, configName, configVersion, deviceInfo, progress);
        }

        public void Ping(string serviceUrl)
        {
            if (!ApplicationContext.Current.Settings.PingDisabled)
            {
                var request = WebRequest.Create(string.Format("{0}/ping", serviceUrl.ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled)));
                request.Timeout = 5000;
                try
                {
                    using (request.GetResponse())
                    {
                    }
                }
                catch (WebException e)
                {
                    // https unavailable, check for regular http connection
                    request = WebRequest.Create(string.Format("{0}/ping", serviceUrl.ToCurrentScheme(true)));
                    request.Timeout = 5000;
                    using (request.GetResponse())
                    {
                    }
                    ApplicationContext.Current.Settings.HttpsDisabled = true;
                }
            }
        }
    }
}
