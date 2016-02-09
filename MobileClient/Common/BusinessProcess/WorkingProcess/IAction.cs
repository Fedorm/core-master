using System;

namespace BitMobile.Common.BusinessProcess.WorkingProcess
{
    public interface IAction
    {
        String Name { get; set; }
        String NextStep { get; set; }
        String NextWorkflow { get; set; }
    }
}