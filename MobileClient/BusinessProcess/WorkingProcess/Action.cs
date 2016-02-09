using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.WorkingProcess
{
    [MarkupElement(MarkupElementAttribute.BusinessProcessNamespace, "Action")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Action : IAction
    {
        public string Name { get; set; }

        public string NextStep { get; set; }

        public string NextWorkflow { get; set; }
    }
}

