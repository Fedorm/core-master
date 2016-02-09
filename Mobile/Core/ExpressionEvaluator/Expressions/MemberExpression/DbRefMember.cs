using BitMobile.DbEngine;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using BitMobile.ValueStack;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class DbRefMember : IMember
    {
        public readonly string DebugString;

        string _member;

        public DbRefMember(string member, string debugString)
        {
            _member = member;

            DebugString = debugString;
        }

        public object Invoke(object obj, object root)
        {
            DbRef dbRef = obj as DbRef;
            if (dbRef == null)
                throw new ArgumentException(string.Format("Invalid type of argument:", obj));

            IEntity result = dbRef.GetObject();
            return result.GetValue(_member);
        }
    }
}
