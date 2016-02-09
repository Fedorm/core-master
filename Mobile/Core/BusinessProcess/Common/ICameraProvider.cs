using System;
using BitMobile.ClientModel;

namespace BitMobile.Common
{
    public interface ICameraProvider
    {
        void MakeSnapshot(string path, int size, Action<object, Camera.CallbackArgs> callback, object state);
    }
}