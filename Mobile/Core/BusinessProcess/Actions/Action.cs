using System;
using System.Collections.Generic;
using System.Text;

using BitMobile.Controls;
using BitMobile.Application;

namespace BitMobile.Actions
{
    public abstract class Action
    {
        protected Dictionary<String,object> parameters = new Dictionary<string, object>();
		protected Dictionary<String,LazyParameterDelegate> lazyParameters = new Dictionary<string, LazyParameterDelegate>();
		private Action nextAction = null;

		protected Dictionary<String,object> Parameters
        {
            get { return parameters; }
        }

		public Action NextAction 
		{
			get { return nextAction; }
		}

        public void AddParameter(String name, object value)
        {
            parameters.Add(name, value);
        }

		public void AddLazyParameter(String name, LazyParameterDelegate value)
		{
			lazyParameters.Add(name, value);
		}

		public static Action CreateObject(IApplicationContext ctx, String value)
        {
			List<Action> actionList = new List<Action>();
			String[] actions = value.Split(';');
			foreach(String s in actions)
			{
	            String[] arr = s.Split(':');
				if (arr.Length != 2)
					throw new Exception (String.Format ("Invalid action: {0}", value));

				Action a = null;
				if(ctx.Workflow.HasAction(arr[0]))
				   a = new WorkflowAction(arr[0]);
				else
				{
		            Type t = typeof(Action).Assembly.GetType(String.Format("{0}.{1}", "BitMobile.Actions", arr[0]));
					if(t==null)
						throw new Exception("Invalid action: " + arr[0]); 
		            System.Reflection.ConstructorInfo ci = t.GetConstructor(new Type[] { });
		            a = (Action)ci.Invoke(new object[] { });
				}
	            if (arr.Length > 1)
	            {
					int n = 0;
	                foreach (String v in arr[1].Split(','))
	                {
						String paramName = String.Format("param{0}",(n+1).ToString());
	                    String paramValue = v.Trim();
						if (paramValue.StartsWith("$"))
						{
							if(LazyParameter.LazyExpression(ctx.ValueStack,paramValue))
								a.AddLazyParameter(paramName, delegate()								                   {
									return ctx.ValueStack.Evaluate(paramValue,null);
								});
							else
								a.AddParameter(paramName, ctx.ValueStack.Evaluate(paramValue,null));
						}
						else
	                    	a.AddParameter(paramName, paramValue);
						n++;
	                }
	            }
				actionList.Add(a);
			}

			for(int i=0;i<actionList.Count-1;i++)
			{
				actionList[i].nextAction = actionList[i+1];
			}


			ctx.Workflow.RegisterAction(actionList[0]);
            return actionList[0];
        }

        public virtual void Invoke(IApplicationContext context)
		{
			EvaluateLazyParameters();
			context.CurrentScreen.Screen.ExitEditMode();
		}

		public void InvokeAll(IApplicationContext context)
		{
			Action a = this;
			while(a!=null)
			{
				a.Invoke(context);
				a = a.NextAction;
			}
		}

		private void EvaluateLazyParameters()
		{
			foreach(KeyValuePair<String,LazyParameterDelegate> pair in lazyParameters)
			{
				parameters.Remove(pair.Key);
				parameters.Add(pair.Key,pair.Value());
			}
		}

		public bool ContainsWorkflowAction(String name)
		{
			Action a = this;
			while(a!=null)
			{
				if(a is WorkflowAction)
				{
					if(((WorkflowAction)a).Name.Equals(name))
						return true;
				}
				a = a.NextAction;
			}
			return false;
		}
    }

	public delegate object LazyParameterDelegate();
}