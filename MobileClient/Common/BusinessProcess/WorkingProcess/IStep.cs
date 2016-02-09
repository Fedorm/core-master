using System;
using System.Collections.Generic;
using BitMobile.Common.Controls;

namespace BitMobile.Common.BusinessProcess.WorkingProcess
{
    public interface IStep
    {
        string Name { get; set; }
        string Screen { get; set; }
        string Controller { get; set; }
        Dictionary<string, IAction> Actions { get; }
        Dictionary<string, object> Parameters { get; set; }
        Dictionary<string, object> State { get; }
        object[] Controls { get; }
        IStep Clone();
        void AddChild(object obj);
        object GetControl(int index);
        void Init();
        void SaveControlsState(IDictionary<string, IPersistable> controls);
    }
}
