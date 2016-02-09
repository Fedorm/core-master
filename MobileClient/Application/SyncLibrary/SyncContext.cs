using BitMobile.Common.SyncLibrary;

namespace BitMobile.Application.SyncLibrary
{
    public static class SyncContext
    {
        public static ISyncContext Current { get; private set; }

        public static void Init(ISyncContext context)
        {
            Current = context;
        }
    }
}
