using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class MemberExpression<T> : IExpression<T>
    {
        public readonly string DebugString;

        List<IMember> _members;

        public MemberExpression(string debugString)
        {
            DebugString = debugString;

            _members = new List<IMember>();
        }

        public void Add(IMember member)
        {
            _members.Add(member);
        }

        public T Evaluate(object root)
        {
            if (root == null)
                throw new ArgumentNullException("Root object cannot be null. Expression: " + DebugString);

            object obj = root;
            foreach (var member in _members)
                obj = member.Invoke(obj, root);

            T result;
            if (obj is IConvertible)
                result = (T)Convert.ChangeType(obj, typeof(T));
            else
                result = (T)obj;

            return result;
        }
    }
}
