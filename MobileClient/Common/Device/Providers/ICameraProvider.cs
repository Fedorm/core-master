using System;

namespace BitMobile.Common.Device.Providers
{
    public interface ICameraProvider
    {
        void MakeSnapshot(string path, int size, Action<bool> callback);
    }
}