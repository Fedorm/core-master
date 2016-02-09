using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Application.IO;
using BitMobile.Common.Application;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.WorkingProcess
{
    [MarkupElement(MarkupElementAttribute.BusinessProcessNamespace, "BusinessProcess")]
    public class BusinessProcess : IContainer, IBusinessProcess
    {
        readonly Stack<Workflow> _workflowStack;
        readonly Dictionary<String, Workflow> _workflows;
        Workflow _firstWorkflow;

        public BusinessProcess()
        {
            _workflows = new Dictionary<string, Workflow>();
            _workflowStack = new Stack<Workflow>();
        }

        public IWorkflow Workflow
        {
            get
            {
                if (_workflowStack.Count > 0)
                    return _workflowStack.Peek();
                return null;
            }
        }

        public bool AllowStatePersist { get; internal set; }

        public void Start(IApplicationContext ctx, String workflowName = null)
        {
            if (workflowName == null)
                OnStartApplication(ctx);

            _workflowStack.Push(workflowName == null ? _firstWorkflow : _workflows[workflowName]);
            Workflow.Start(ctx);
        }

        private void OnStartApplication(IApplicationContext ctx)
        {
            if (ctx.Settings.ClearCacheOnStart)
                IOContext.Current.Delete(ctx.LocalStorage);
        }

        public void TerminateWorkflow(IApplicationContext ctx, IWorkflow wf, bool rollback = false)
        {
            if (rollback)
                ctx.Dal.CancelChanges();
            else
                ctx.Dal.SaveChanges();

            Workflow lastWorkflow = _workflowStack.Pop();
            lastWorkflow.Stop(ctx);

            if (_workflowStack.Count == 0)
                throw new Exception("Application terminated");
        }

        #region IContainer

        public void AddChild(object obj)
        {
            var wf = (Workflow)obj;
            if (_firstWorkflow == null)
                _firstWorkflow = wf;
            wf.SetBusinessProcess(this);
            _workflows.Add(wf.Name, wf);
        }

        public object[] Controls
        {
            // ReSharper disable once CoVariantArrayConversion
            get { return _workflows.Values.ToArray(); }
        }

        public object GetControl(int index)
        {
            return _workflows.Values.ToArray()[index];
        }
        #endregion
    }
}

