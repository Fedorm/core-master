using System;
using BitMobile.ClientModel;

namespace BitMobile.Common
{
    public interface IGalleryProvider
    {
        void Copy(string path, int size, Action<object, Gallery.CallbackArgs> callback, object state);
    }
}

