using BitMobile.Common.Debugger;


namespace BitMobile.Application.Debugger
{
    public static class DebugContext
    {
        public static IDebugContext Current { get; private set; }

        public static void Init(IDebugContext context)
        {
            Current = context;
        }
    }
}