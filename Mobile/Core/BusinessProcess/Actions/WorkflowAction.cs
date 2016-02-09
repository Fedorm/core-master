using System;

namespace BitMobile.Actions
{
	public class WorkflowAction : Action
	{
		private String name;

		public WorkflowAction (String name)
		{
			this.name = name;
		}

		public override void Invoke(BitMobile.Application.IApplicationContext context)
		{
			base.Invoke(context);
			context.Workflow.InvokeAction(context,name,parameters);
		}

		public String Name {
			get {
				return name;
			}
		}
	}
}

