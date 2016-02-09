using BitMobile.Common.StyleSheet;

namespace BitMobile.Application.StyleSheet
{
    public static class StyleSheetContext
    {
        public static IStyleSheetContext Current { get; private set; }

        public static void Init(IStyleSheetContext context)
        {
            Current = context;
        }
    }
}