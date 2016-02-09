using System;
using System.Diagnostics;

namespace BitMobile.ExpressionEvaluator.Expressions
{
    [DebuggerDisplay("{DebugString}")]
    class ObjectExpression<T> : IExpression<T>
    {
        public readonly string DebugString;

        T _value;

        public ObjectExpression(object value, string expression)
        {
            if (value is IConvertible)
                _value = (T)Convert.ChangeType(value, typeof(T));
            else
                _value = (T)value;

            DebugString = expression;
        }

        public T Evaluate(object root)
        {
            return _value;
        }
    }
}