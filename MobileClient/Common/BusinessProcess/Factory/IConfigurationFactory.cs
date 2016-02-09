using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.BusinessProcess.Factory
{
    public interface IConfigurationFactory
    {
        IConfiguration CreateConfiguration(IValueStack stack);
    }
}
