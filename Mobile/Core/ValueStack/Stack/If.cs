using System;
using System.Collections.Generic;
using System.Text;


namespace BitMobile.ValueStack
{
    public class If : ValueStackTag
    {
        private String test;

        public If()
        {
        }

        public String Test
        {
            get { return test; }
            set { test = value; }
        }

		public virtual bool Evaluate(object value)
		{
			return (bool)value;
		}
    }

	public class Else : ValueStackTag
	{
		public Else()
		{
		}
	}

	public class IfNotNull : If
	{
		public override bool Evaluate(object value)
		{
			return value!=null;
		}
	}

	public class IfNull : If
	{
		public override bool Evaluate(object value)
		{
			return value==null;
		}
	}


}