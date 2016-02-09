using System;

namespace BitMobile.Controls
{
	public class SubmitButton: Button
	{
		public SubmitButton ()
		{
			Scope = string.Empty;
		}

		public string Scope{ get; set; }

		protected override bool InvokeClick ()
		{
			bool allowed = true;
			if (!string.IsNullOrWhiteSpace(this.Scope)) 
				allowed = _applicationContext.Validate (this.Scope);

			if (allowed) 
				return base.InvokeClick ();

			return false;
		}
	}
}

