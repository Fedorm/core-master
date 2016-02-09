using System;

namespace BitMobile.Actions
{
	public static class LazyParameter
	{
		public static bool LazyExpression(BitMobile.ValueStack.ValueStack stack, String expression)
		{
			expression = expression.Replace("\"", "").Trim();
			if (expression.StartsWith("$"))
			{
				String[] parts = expression.Split('.');
				if(!parts[0].StartsWith("$"))
					throw new Exception("Invalid expression: " + expression);
				String root = parts[0].Remove(0, 1);
				
				if(stack.Values.ContainsKey(root))
				{
					object obj = stack.Values[root];
					if(obj is BitMobile.Controls.IDataBind)
						return true;
				}
				
			}
			return false;
		}
	}
}