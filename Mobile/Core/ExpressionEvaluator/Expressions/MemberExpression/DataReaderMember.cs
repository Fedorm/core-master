using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using BitMobile.DbEngine;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    [DebuggerDisplay("{DebugString}")]
    class DataReaderMember : IMember
    {
        public readonly string DebugString;

        string _field;

        public DataReaderMember(string field, string debugString)
        {
            _field = field;

            DebugString = debugString;
        }

        public object Invoke(object obj, object root)
        {
            object result;
            IDbRecordset reader = obj as IDbRecordset;
            if (reader == null)
                throw new ArgumentException(string.Format("Invalid type of argument: {0}", obj));

            result = ((IDbRecordset)obj).GetValue(_field);
            /*
            int idx = reader.GetOrdinal(_field);
            if (idx == -1)
                throw new Exception(String.Format("Recordset does not contain field '{0}'", _field));
            result = reader.GetValue(idx);
            */
            if (result is DbEngine.DbRef)
                result = ((DbEngine.DbRef)result).GetObject();

            return result;
        }
    }
}
