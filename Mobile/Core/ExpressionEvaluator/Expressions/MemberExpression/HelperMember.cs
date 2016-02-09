using System;
using System.Diagnostics;
using System.Reflection;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class HelperMember : IMember
    {
        public readonly string DebugString;

        MethodInfo _methodInfo;
        IExpression<object>[] _parameters;

        public HelperMember(MethodInfo methodInfo, IExpression<object>[] parameters, string debugString)
        {
            _methodInfo = methodInfo;
            _parameters = parameters;

            DebugString = debugString;
        }

        public object Invoke(object obj, object root)
        {
            object[] paramsValues = new object[_parameters.Length + 1];
            paramsValues[0] = obj;

            for (int i = 0; i < _parameters.Length; i++)
            {
                object value = _parameters[i].Evaluate(root);

                Type paramType = _methodInfo.GetParameters()[i + 1].ParameterType;

                if (value is IConvertible)
                    paramsValues[i + 1] = Convert.ChangeType(value, paramType);
                else
                    paramsValues[i + 1] = value;
            }
            
            return _methodInfo.Invoke(null, paramsValues);
        }
    }
}
