using System;
using BitMobile.Common.Application;

namespace BitMobile.Common.BusinessProcess.WorkingProcess
{
    public interface IBusinessProcess
    {
        IWorkflow Workflow { get; }
        object[] Controls { get; }
        bool AllowStatePersist { get; }
        void Start(IApplicationContext ctx, String workflowName = null);
        void TerminateWorkflow(IApplicationContext ctx, IWorkflow wf, bool rollback = false);
        void AddChild(object obj);
        object GetControl(int index);
    }
}