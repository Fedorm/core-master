using System.Collections.Generic;
using System.Linq;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.WorkingProcess
{
    [MarkupElement(MarkupElementAttribute.BusinessProcessNamespace, "Step")]
    public class Step : IContainer, IStep
    {
        public Step()
        {
            Actions = new Dictionary<string, IAction>();
            State = new Dictionary<string, object>();
        }

        public string Name { get; set; }

        public string Screen { get; set; }

        public string Controller { get; set; }

        public Dictionary<string, IAction> Actions { get; private set; }
        
        public Dictionary<string, object> Parameters { get; set; }

        public Dictionary<string, object> State { get; private set; }

        public IStep Clone()
        {
            var step = new Step { Name = Name, Screen = Screen, Controller = Controller };

            foreach (var action in Actions)
                step.Actions.Add(action.Key, action.Value);

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
        }

        public void SaveControlsState(IDictionary<string, IPersistable> controls)
        {
            foreach (var control in controls)
                State[control.Key] = control.Value.GetState();
        }
    }
}

