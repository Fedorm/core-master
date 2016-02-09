using System;

namespace BitMobile.Common.SyncLibrary
{
    public interface ICacheController
    {
        ICacheControllerBehavior ControllerBehavior { get; }
        void RefreshAsync();
        event Action<Exception> RefreshCompleted;
    }
}
