using System;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.BusinessProcess.Factory
{
    public interface IBusinessProcessFactory
    {
        IBusinessProcess CreateBusinessProcess(String bpName, IValueStack stack);
    }
}
