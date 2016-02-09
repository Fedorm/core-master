using System;
using System.Diagnostics;
using System.Reflection;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class MethodMember : IMember
    {
        public readonly string DebugString;

        MethodInfo _methodInfo;
        IExpression<object>[] _parameters;

        public MethodMember(MethodInfo methodInfo, IExpression<object>[] parameters, string debugString)
        {
            _methodInfo = methodInfo;
            _parameters = parameters;

            DebugString = debugString;
        }

        public object Invoke(object obj, object root)
        {
            object[] paramsValues = new object[_parameters.Length];

            for (int i = 0; i < _parameters.Length; i++)
            {
                object value = _parameters[i].Evaluate(root);

                Type paramType = _methodInfo.GetParameters()[i].ParameterType;

                if (value is IConvertible)
                    paramsValues[i] = Convert.ChangeType(value, paramType);
                else
                    paramsValues[i] = value;
            }

            return _methodInfo.Invoke(obj, paramsValues);
        }
    }
}
