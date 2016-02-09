using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.BusinessProcess.Factory;
using BitMobile.Common.Application;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.WorkingProcess
{
    [MarkupElement(MarkupElementAttribute.BusinessProcessNamespace, "Workflow")]
    public class Workflow : IContainer, IWorkflow
    {
        const string WorkflowStartEvent = "OnWorkflowStart";
        const string WorkflowForwardingEvent = "OnWorkflowForwarding";
        const string WorkflowForwardEvent = "OnWorkflowForward";
        const string WorkflowBackEvent = "OnWorkflowBack";
        const string WorkflowFinishEvent = "OnWorkflowFinish";
        const string WorkflowFinishedEvent = "OnWorkflowFinished";
        const string WorkflowPauseEvent = "OnWorkflowPause";

        const string WorkflowCommitReason = "commit";
        const string WorkflowRollbackReason = "rollback";

        private readonly Dictionary<String, Step> _steps = new Dictionary<string, Step>();
        private readonly List<String> _globalActions = new List<string> { "Back", "BackTo", "Commit", "Rollback" };
        private readonly Stack<Step> _history = new Stack<Step>();
        private Step _firstStep;
        private Step _currentStep;
        private BusinessProcess _businessProcess;

        public IBusinessProcess BusinessProcess
        {
            get { return _businessProcess; }
        }

        public String Name { get; set; }

        public String Controller { get; set; }

        public IStep CurrentStep
        {
            get
            {
                return _currentStep;
            }
        }

        public void SetBusinessProcess(BusinessProcess businessProcess)
        {
            _businessProcess = businessProcess;
        }

        public bool HasAction(String name)
        {
            return _currentStep.Actions.ContainsKey(name) || _globalActions.Contains(name);
        }

        public void Start(IApplicationContext ctx, bool rollBack = false)
        {
            InvokeCallback(WorkflowStartEvent);
            LogManager.Logger.WorkflowStarted(Name);

            Step step = _currentStep ?? (Step)_firstStep.Clone();
            DoForward(ctx, step, null, rollBack);
        }

        public void Stop(IApplicationContext ctx)
        {
        }

        public void InvokeAction(IApplicationContext ctx
            , string name
            , Dictionary<string, object> parameters
            , bool isBackCommand = false)
        {
            try
            {
                if (_currentStep != null)
                {
                    _currentStep.SaveControlsState(ctx.ValueStack.Persistables);

                    if (_currentStep.Actions.ContainsKey(name))
                    {
                        IAction a = _currentStep.Actions[name];

                        _businessProcess.AllowStatePersist = false;
                        if (!string.IsNullOrEmpty(a.NextStep))
                        {
                            DoForward(ctx, _steps[a.NextStep], parameters, isBackCommand);
                            return;
                        }

                        if (!string.IsNullOrEmpty(a.NextWorkflow))
                        {
                            InvokeCallback(WorkflowPauseEvent);
                            LogManager.Logger.WorkflowPaused();
                            _businessProcess.Start(ctx, a.NextWorkflow);
                            return;
                        }

                        // We do nothing. For backward compatibility.
                        return;
                    }
                }

                if (_globalActions.Contains(name))
                {
                    _businessProcess.AllowStatePersist = true;

                    switch (name)
                    {
                        case "Back":
                            DoBack(ctx);
                            break;
                        case "BackTo":
                            DoBack(ctx, parameters["step"].ToString());
                            break;
                        case "Commit":
                            Finish(ctx, false);
                            break;
                        case "Rollback":
                            Finish(ctx, true);
                            break;
                    }

                    return;
                }

                Console.WriteLine("Empty action: '{0}'", name);

                ControlsContext.Current.ActionHandlerLocker.Release();
            }
            catch (Exception e)
            {
                ctx.ValueStack.ExceptionHandler.Handle(e);
            }
        }

        public void Refresh(IApplicationContext ctx, Dictionary<String, object> parameters)
        {
            _businessProcess.AllowStatePersist = true;

            _currentStep.SaveControlsState(ctx.ValueStack.Persistables);

            ctx.RefreshScreen(parameters);
        }

        #region IContainer

        public void AddChild(object obj)
        {
            var step = (Step)obj;
            if (_firstStep == null)
                _firstStep = step;
            _steps.Add(step.Name, step);
        }

        public object[] Controls
        {
            // ReSharper disable once CoVariantArrayConversion
            get { return _steps.Values.ToArray(); }
        }

        public object GetControl(int index)
        {
            return _steps.Values.ToArray()[index];
        }
        #endregion

        void DoForward(IApplicationContext ctx, Step step, Dictionary<String, object> parameters = null, bool isBackCommand = false)
        {
            string lastStep = _currentStep != null ? _currentStep.Name : "null";
            string nextStep = step != null ? step.Name : "null";
            object allowed = InvokeCallback(WorkflowForwardingEvent
                  , lastStep
                  , nextStep
                  , parameters);

            if (allowed != null && !(allowed is bool))
                throw new JsException(string.Format(
                    "Function {0} returned incorrect value: {1}", WorkflowForwardingEvent, allowed));

            if (allowed == null || (bool)allowed)
            {
                LogManager.Logger.WorkflowForward(nextStep, parameters);

                OpenScreen(ctx, step, parameters, isBackCommand);

                InvokeCallback(WorkflowForwardEvent
                    , lastStep
                    , nextStep
                    , parameters);
            }
            else
            {
                LogManager.Logger.WorkflowForwardNotAllowed(nextStep, parameters);
                ControlsContext.Current.ActionHandlerLocker.Release();
            }
        }

        void DoBack(IApplicationContext ctx)
        {
            _history.Pop(); //remove current
            if (_history.Count > 0)
            {
                Step step = _history.Pop();
                InvokeCallback(WorkflowBackEvent
                    , _currentStep != null ? _currentStep.Name : "null"
                    , step != null ? step.Name : "null");

                LogManager.Logger.WorkflowBack();

                OpenScreen(ctx, step, step.Parameters, true);
            }
            else
            {
                Finish(ctx, true);
            }
        }


        void DoBack(IApplicationContext ctx, string toStep)
        {
            _history.Pop(); //remove current
            Step step = null;
            bool flag = false;
            while (_history.Count > 0)
            {
                step = _history.Pop();
                if (step.Name.ToLower().Equals(toStep.ToLower()))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
                throw new Exception(String.Format("Back command failed. Step '{0}' is not found in history", toStep));

            InvokeCallback(WorkflowBackEvent, _currentStep != null ? _currentStep.Name : "null", step.Name);

            LogManager.Logger.WorkflowBackTo(toStep);

            OpenScreen(ctx, step, step.Parameters, true);
        }

        void Finish(IApplicationContext ctx, bool rollback)
        {
            string reason = rollback ? WorkflowRollbackReason : WorkflowCommitReason;

            InvokeCallback(WorkflowFinishEvent, reason);
            LogManager.Logger.WorkflowFinished(reason);

            ClearState();
            _businessProcess.TerminateWorkflow(ctx, this, rollback);

            InvokeCallback(WorkflowFinishedEvent, reason);

            _businessProcess.Workflow.Start(ctx, rollback);
        }

        void OpenScreen(IApplicationContext ctx, Step step, Dictionary<String, object> parameters = null, bool isBackCommand = false)
        {
            step.Parameters = parameters;
            if (_currentStep != step)
            {
                step.Init();
                _history.Push(step);
                _currentStep = step;
            }

            string controller = String.IsNullOrEmpty(step.Controller) ? Controller : step.Controller;
            ctx.OpenScreen(step.Screen, controller, step.Parameters, isBackCommand);
        }

        object InvokeCallback(string eventName, params object[] parameters)
        {
            var newParameters = new object[parameters.Length + 1];

            newParameters[0] = Name;
            for (int i = 0; i < parameters.Length; i++)
                newParameters[i + 1] = parameters[i];

            return ControllerFactory.GlobalEvents.InvokeEvent(eventName, newParameters);
        }

        void ClearState()
        {
            _history.Clear();
            _currentStep = null;
        }
    }
}

