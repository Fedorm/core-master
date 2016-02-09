using BitMobile.Common.DbEngine;

namespace BitMobile.Application.DbEngine
{
    public static class DbContext
    {
        public static IDbContext Current { get; private set; }

        public static void Init(IDbContext context)
        {
            Current = context;
        }
    }
}
