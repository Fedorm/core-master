using System.Diagnostics;
using System.Reflection;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class PropertyMember : IMember
    {
        public readonly string DebugString;

        PropertyInfo _propertyInfo;

        public PropertyMember(PropertyInfo propertyInfo, string itemString)
        {
            _propertyInfo = propertyInfo;

            DebugString = itemString;
        }

        public object Invoke(object obj, object root)
        {
            return _propertyInfo.GetValue(obj);
        }
    }
}
