using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "BusinessProcess")]
    public class BusinessProcess : IBusinessProcess
    {
        public string File { get; set; }
    }
}

