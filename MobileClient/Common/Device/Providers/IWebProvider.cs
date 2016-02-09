using System;

namespace BitMobile.Common.Device.Providers
{
    public interface IWebProvider
    {
        void OpenUrl(Uri url);
    }
}