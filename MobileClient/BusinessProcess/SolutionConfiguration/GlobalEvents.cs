using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "GlobalEvents")]
    public class GlobalEvents: IGlobalEvents
    {
        // ReSharper disable once UnusedMember.Global
        public string File { get; set; }
    }
}

