using BitMobile.Common.BusinessProcess;

namespace BitMobile.Application.BusinessProcess
{
    public static class BusinessProcessContext
    {
        public static IBusinessProcessContext Current { get; private set; }

        public static void Init(IBusinessProcessContext context)
        {
            Current = context;
        }
    }
}
