using BitMobile.Common.Application;

namespace BitMobile.Application
{
    public static class ApplicationContext
    {
        public static IApplicationContext Current { get; private set; }

        public static void Init(IApplicationContext ctx)
        {
            Current = ctx;
        }
    }
}

