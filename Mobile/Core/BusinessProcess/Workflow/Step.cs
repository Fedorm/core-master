using System.Collections.Generic;
using System.Linq;
using BitMobile.Controls;
using Common.Controls;

namespace BitMobile.BusinessProcess
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global, ClassNeverInstantiated.Global
    public class Step : IContainer
    {
        public Step()
        {
            RegisteredActions = new List<Actions.Action>();
            Actions = new Dictionary<string, Action>();
            State = new Dictionary<string, object>();
        }

        public string Name { get; set; }

        public string Screen { get; set; }

        public string Controller { get; set; }

        public Dictionary<string, Action> Actions { get; private set; }

        public List<Actions.Action> RegisteredActions { get; private set; }

        public Dictionary<string, object> Parameters { get; set; }

        public Dictionary<string, object> State { get; private set; }

        public Step Clone()
        {
            var step = new Step { Name = Name, Screen = Screen, Controller = Controller };

            foreach (var action in Actions)
                step.Actions.Add(action.Key, action.Value);

            foreach (var action in RegisteredActions)
                step.RegisteredActions.Add(action);

            if (Parameters != null)
                foreach (var parameter in Parameters)
                    step.Parameters.Add(parameter.Key, parameter.Value);

            return step;
        }

        public void AddChild(object obj)
        {
            var a = (Action)obj;
            Actions.Add(a.Name, a);
        }

        public object[] Controls
        {
            // ReSharper disable once CoVariantArrayConversion
            get { return Actions.Values.ToArray(); }
        }

        public object GetControl(int index)
        {
            return Actions.Values.ToArray()[index];
        }

        public void Init()
        {
            RegisteredActions.Clear();
        }

        public void SaveControlsState(Dictionary<string, IPersistable> controls)
        {
            foreach (var control in controls)
                State[control.Key] = control.Value.GetState();
        }
    }
}

