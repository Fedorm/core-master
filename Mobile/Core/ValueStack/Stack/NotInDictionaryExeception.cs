using System;
using System.Linq.Expressions;

namespace BitMobile.ValueStack
{
	public class NotInDictionaryExeception : Exception
	{
		private Expression expression;

		public NotInDictionaryExeception (Expression exp)
		{
			this.expression = exp;
		}

		public Expression Expression {
			get {
				return expression;
			}
		}
	}
}

