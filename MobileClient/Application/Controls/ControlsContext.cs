using BitMobile.Common.Controls;

namespace BitMobile.Application.Controls
{
    public static class ControlsContext
    {
        public static IControlsContext Current { get; private set; }

        public static void Init(IControlsContext context)
        {
            Current = context;
        }
    }
}
