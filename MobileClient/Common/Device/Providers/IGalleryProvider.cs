using System;

namespace BitMobile.Common.Device.Providers
{
    public interface IGalleryProvider
    {
        void Copy(string path, int size, Action<bool> callback);
    }
}

