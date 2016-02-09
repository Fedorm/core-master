using BitMobile.Application;
using BitMobile.Controls;
using BitMobile.ValueStack;
using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.Translator;
using Common.Controls;

namespace BitMobile.BusinessProcess
{
    public class Workflow : IContainer
    {
        const string WORKFLOW_START_EVENT = "OnWorkflowStart";
        const string WORKFLOW_FORWARDING_EVENT = "OnWorkflowForwarding";
        const string WORKFLOW_FORWARD_EVENT = "OnWorkflowForward";
        const string WORKFLOW_BACK_EVENT = "OnWorkflowBack";
        const string WORKFLOW_FINISH_EVENT = "OnWorkflowFinish";
        const string WORKFLOW_PAUSE_EVENT = "OnWorkflowPause";

        const string WORKFLOW_COMMIT_REASON = "commit";
        const string WORKFLOW_ROLLBACK_REASON = "rollback";

        private Dictionary<String, Step> _steps = new Dictionary<string, Step>();
        private Step _firstStep;
        private Step _currentStep;
        private List<String> _globalActions = new List<string>() { "Back", "BackTo", "Commit", "Rollback" };
        private Stack<Step> _history = new Stack<Step>();

        public Workflow()
        {
        }

        public BusinessProcess BusinessProcess { get; set; }

        public List<BitMobile.Actions.Action> RegisteredActions
        {
            get
            {
                return _currentStep.RegisteredActions;
            }
        }

        public String Name { get; set; }
        public String Controller { get; set; }

        public Step CurrentStep
        {
            get
            {
                return _currentStep;
            }
        }

        public bool HasAction(String name)
        {
            return _currentStep.Actions.ContainsKey(name) || _globalActions.Contains(name);
        }

        public void Start(IApplicationContext ctx, bool rollBack = false)
        {
            InvokeCallback(ctx, WORKFLOW_START_EVENT);

            Step step = _currentStep ?? _firstStep.Clone();
            DoForward(ctx, step, null, rollBack);
        }

        public void Stop(IApplicationContext ctx)
        {
        }

        public void RegisterAction(BitMobile.Actions.Action a)
        {
            _currentStep.RegisteredActions.Add(a);
        }

        public void InvokeAction(IApplicationContext ctx
            , String name
            , Dictionary<String, object> parameters
            , bool isBackCommand = false)
        {
            try
            {
                if (_currentStep != null)
                    _currentStep.SaveControlsState(ctx.ValueStack.Persistables);

                if (this._currentStep.Actions.ContainsKey(name))
                {
                    Action a = this._currentStep.Actions[name];

                    if (!String.IsNullOrEmpty(a.NextStep))
                    {
                        DoForward(ctx, this._steps[a.NextStep], parameters, isBackCommand);
                        return;
                    }

                    if (!String.IsNullOrEmpty(a.NextWorkflow))
                    {
                        InvokeCallback(ctx, WORKFLOW_PAUSE_EVENT);
                        BusinessProcess.Start(ctx, a.NextWorkflow);
                        return;
                    }

                    BusinessProcess.TerminateWorkflow(ctx, this, false);

                    return;
                }

                if (_globalActions.Contains(name))
                {
                    switch (name)
                    {
                        case "Back":
                            DoBack(ctx);
                            break;
                        case "BackTo":
                            DoBack(ctx, parameters["step"].ToString());
                            break;
                        case "Commit":
                            DoCommit(ctx);
                            break;
                        case "Rollback":
                            DoRollback(ctx);
                            break;
                    }

                    return;
                }

                ActionHandlerEx.Busy = false;
                ActionHandler.Busy = false;
            }
            catch (Exception e)
            {
                ctx.ValueStack.ExceptionHandler.Handle(e);
            }
        }

		public void Refresh(IApplicationContext ctx, Dictionary<String, object> parameters)
		{
			_currentStep.SaveControlsState(ctx.ValueStack.Persistables);

			ctx.RefreshScreen (parameters);
		}

        #region IContainer

        public void AddChild(object obj)
        {
            Step step = (Step)obj;
            if (_firstStep == null)
                _firstStep = step;
            _steps.Add(step.Name, step);
        }

        public object[] Controls
        {
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
            object allowed = InvokeCallback(ctx, WORKFLOW_FORWARDING_EVENT
                  , lastStep
                  , step != null ? step.Name : "null"
                  , parameters);

            if (allowed != null && !(allowed is bool))
                throw new JSException(string.Format(
                    "Function {0} returned incorrect value: {1}", WORKFLOW_FORWARDING_EVENT, allowed));

            if (allowed == null || (bool)allowed)
            {
                OpenScreen(ctx, step, parameters, isBackCommand);

                InvokeCallback(ctx, WORKFLOW_FORWARD_EVENT
                    , lastStep
                    , step != null ? step.Name : "null"
                    , parameters);
            }
        }

        void DoBack(IApplicationContext ctx)
        {
            _history.Pop(); //remove current
            if (_history.Count > 0)
            {
                Step step = _history.Pop();
                InvokeCallback(ctx, WORKFLOW_BACK_EVENT
                    , _currentStep != null ? _currentStep.Name : "null"
                    , step != null ? step.Name : "null");

                OpenScreen(ctx, step, step.Parameters, true);
            }
            else
            {
                InvokeCallback(ctx, WORKFLOW_FINISH_EVENT, WORKFLOW_ROLLBACK_REASON);
                BusinessProcess.TerminateWorkflow(ctx, this, true);
            }
        }


        void DoBack(IApplicationContext ctx, String toStep)
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

            InvokeCallback(ctx, WORKFLOW_BACK_EVENT
                , _currentStep != null ? _currentStep.Name : "null"
                , step != null ? step.Name : "null");

            OpenScreen(ctx, step, step.Parameters, true);
        }

        void DoCommit(IApplicationContext ctx)
        {
            InvokeCallback(ctx, WORKFLOW_FINISH_EVENT, WORKFLOW_COMMIT_REASON);
            ClearState();
            BusinessProcess.TerminateWorkflow(ctx, this, false);
        }

        void DoRollback(IApplicationContext ctx)
        {
            InvokeCallback(ctx, WORKFLOW_FINISH_EVENT, WORKFLOW_ROLLBACK_REASON);
            ClearState();
            BusinessProcess.TerminateWorkflow(ctx, this, true);
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

        object InvokeCallback(IApplicationContext ctx, string eventName, params object[] parameters)
        {
            object[] newParameters = new object[parameters.Length + 1];

            newParameters[0] = this.Name;
            for (int i = 0; i < parameters.Length; i++)
                newParameters[i + 1] = parameters[i];

            return BitMobile.Factory.ControllerFactory.GlobalEvents.InvokeEvent(eventName, newParameters);
        }

        void ClearState()
        {
            _history.Clear();
            _currentStep = null;
        }
    }
}

