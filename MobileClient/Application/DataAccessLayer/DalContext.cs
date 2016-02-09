using BitMobile.Common.DataAccessLayer;

namespace BitMobile.Application.DataAccessLayer
{
    public static class DalContext
    {
        public static IDalContext Current { get; private set; }

        public static void Init(IDalContext context)
        {
            Current = context;
        }
    }
}