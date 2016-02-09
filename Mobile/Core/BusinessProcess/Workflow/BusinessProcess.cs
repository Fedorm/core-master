using System;
using System.Collections.Generic;
using BitMobile.Application;
using BitMobile.Controls;
using System.Linq;
using System.IO;
using BitMobile.Utilities.IO;

namespace BitMobile.BusinessProcess
{
    public class BusinessProcess : IContainer
    {
        Stack<Workflow> _workflowStack;
        Workflow _firstWorkflow;
        Dictionary<String, Workflow> _workflows;

        public BusinessProcess()
        {
            _workflows = new Dictionary<string, BitMobile.BusinessProcess.Workflow>();
            _workflowStack = new Stack<BitMobile.BusinessProcess.Workflow>();
        }

        public Workflow Workflow
        {
            get
            {
                if (_workflowStack.Count > 0)
                    return _workflowStack.Peek();
                return null;
            }
        }

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
                FileSystemProvider.DeleteDirectory(ctx.LocalStorage);
        }

        public void TerminateWorkflow(IApplicationContext ctx, Workflow wf, bool rollback = false)
        {
            if (rollback)
                ctx.DAL.CancelChanges();
            else
                ctx.DAL.SaveChanges();

            Workflow lastWorkflow = _workflowStack.Pop();
            lastWorkflow.Stop(ctx);

            if (_workflowStack.Count == 0)
                throw new Exception("Application terminated");
            else
            {
                Workflow.Start(ctx, rollback);
            }
        }

        #region IContainer

        public void AddChild(object obj)
        {
            Workflow wf = (Workflow)obj;
            if (_firstWorkflow == null)
                _firstWorkflow = wf;
            wf.BusinessProcess = this;
            _workflows.Add(wf.Name, wf);
        }

        public object[] Controls
        {
            get { return _workflows.Values.ToArray(); }
        }

        public object GetControl(int index)
        {
            return _workflows.Values.ToArray()[index];
        }
        #endregion
    }
}

