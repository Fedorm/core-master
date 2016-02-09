using System;

namespace BitMobile.Common.SyncLibrary
{
    public interface IOfflineContext : IDisposable
    {
        void LoadAsync();
        event Action<Exception> LoadCompleted;
        void ClearCache();
        ICacheController CacheController { get; }
    }
}
