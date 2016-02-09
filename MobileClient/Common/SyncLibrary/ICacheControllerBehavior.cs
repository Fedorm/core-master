using System;
using System.Collections.Generic;
using System.Net;

namespace BitMobile.Common.SyncLibrary
{
    public interface ICacheControllerBehavior
    {
        Action<int, int> ReadProgressCallback { get; set; }
        ICredentials Credentials { get; set; }
        string ConfigName { get; set; }
        string ConfigVersion { get; set; }
        Version CoreVersion { get; set; }
        IDictionary<string, string> DeviceInfo { get; set; }
        string UserId { get; }
        string UserEmail { get; }
        string ResourceVersion { get; }
        void ClearUserSession();
    }
}
