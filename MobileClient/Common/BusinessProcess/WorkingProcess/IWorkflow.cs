using System;
using System.Collections.Generic;
using BitMobile.Common.Application;

namespace BitMobile.Common.BusinessProcess.WorkingProcess
{
    public interface IWorkflow
    {
        IBusinessProcess BusinessProcess { get; }
        String Name { get; set; }
        String Controller { get; set; }
        IStep CurrentStep { get; }
        object[] Controls { get; }
        bool HasAction(String name);
        void Start(IApplicationContext ctx, bool rollBack = false);
        void Stop(IApplicationContext ctx);

        void InvokeAction(IApplicationContext ctx
            , String name
            , Dictionary<String, object> parameters
            , bool isBackCommand = false);

        void Refresh(IApplicationContext ctx, Dictionary<String, object> parameters);
        void AddChild(object obj);
        object GetControl(int index);
    }
}