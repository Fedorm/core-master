using System;

namespace BitMobile.BusinessProcess
{
	public class Action
	{
		private String name;
		private String nextStep;
		private String nextWorkflow;

		public Action ()
		{
		}

		public String Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public String NextStep {
			get {
				return nextStep;
			}
			set {
				nextStep = value;
			}
		}

		public String NextWorkflow {
			get {
				return nextWorkflow;
			}
			set {
				nextWorkflow = value;
			}
		}
	}
}

