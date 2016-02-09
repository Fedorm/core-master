using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    /// <summary>
    /// Необходим в случае, если на момент построения известно значение члена объекта.
    /// В таком случае надобность в Methodmember, PropertyMember отпадает
    /// </summary>
    [DebuggerDisplay("{DebugString}")]
    class ValueMember : IMember
    {
        public readonly string DebugString;

        object _value;

        public ValueMember(object value, string itemString)
        {
            _value = value;

            DebugString = itemString;
        }

        public object Invoke(object obj, object root)
        {
            return _value;
        }
    }
}
