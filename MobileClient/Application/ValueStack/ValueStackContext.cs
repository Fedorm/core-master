using BitMobile.Common.ValueStack;

namespace BitMobile.Application.ValueStack
{
    public static class ValueStackContext
    {
        public static IValueStackContext Current { get; private set; }

        public static void Init(IValueStackContext context)
        {
            Current = context;
        }
    }
}
