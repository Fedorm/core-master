using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.ExpressionEvaluator
{
    class Executor
    {
        private readonly IValueStack _valueStack;
        private readonly ITranslator _translator;
        private IExternalFunction _controller;

        public Executor(IValueStack valueStack, ITranslator translator)
        {
            _valueStack = valueStack;
            _translator = translator;
        }

        public void SetController(IExternalFunction controller)
        {
            _controller = controller;
        }

        public object ExecuteFunction(string identifier, object[] args)
        {
            return _controller.CallFunction(identifier, args);
        }

        public bool TryGetVariable(string identifier, out object obj)
        {
            return _valueStack.Values.TryGetValue(identifier, out obj);
        }

        public object GetControllerVariable(string name)
        {
            return _controller.CallVariable(name);
        }

        public string TranslateByKey(string key)
        {
            return _translator.TranslateByKey(key);
        }

        public object ExecuteMember(object root, string identifier)
        {
            var recordset = root as IDbRecordset;
            if (recordset != null)
                return recordset.GetValue(identifier);

            // dereference of DbRef
            var dbRef = root as IDbRef;
            if (dbRef != null)
                root = dbRef.GetObject();

            // ugly hack for backward compatibility
            if (root == null)
                return null;

            var indexedProperty = root as IIndexedProperty;
            if (indexedProperty != null)
            {
                var ip = indexedProperty;
                if (!ip.HasProperty(identifier))
                    return null;
                return ip.GetValue(identifier);
            }

            var dict = root as IDictionary<string, object>;
            if (dict != null)
            {
                object result;
                if (dict.TryGetValue(identifier, out result))
                    return result;
                return null;
            }

            PropertyInfo pi = root.GetType().GetProperty(identifier);
            if (pi != null)
                return pi.GetValue(root);

            return null;
        }

        public bool ConvertToBoolean(object left)
        {
            var s = left as string;
            if (s != null)
            {
                bool result;
                if (bool.TryParse(s, out result))
                {
                    return result;
                }
                else
                {
                    left = int.Parse(s); // int 0 or 1 may come as string
                }
            }
            if (left is bool)
                return (bool)left;
            if (left is IConvertible)
                return (bool)Convert.ChangeType(left, typeof(bool));

            throw new InvalidCastException("Cannot cast " + left + " to boolean");
        }

        public bool ExecuteBoolean(object left, object right, ExpressionParser.Operator op)
        {
            switch (op)
            {
                case ExpressionParser.Operator.And:
                    return ConvertToBoolean(left) && ConvertToBoolean(right);
                case ExpressionParser.Operator.Or:
                    return ConvertToBoolean(left) || ConvertToBoolean(right);
                case ExpressionParser.Operator.Equality:
                    return Equality(left, right);
                case ExpressionParser.Operator.Inequality:
                    return !Equality(left, right);
                case ExpressionParser.Operator.Less:
                    return Compare(left, right) < 0;
                case ExpressionParser.Operator.Greater:
                    return Compare(left, right) > 0;
                case ExpressionParser.Operator.LessOrEqual:
                    return Compare(left, right) <= 0;
                case ExpressionParser.Operator.GreaterOrEqual:
                    return Compare(left, right) >= 0;
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        private static bool Equality(object left, object right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            if (left.GetType() == right.GetType())
                return left.Equals(right);

            if (!TryParseConst(right, left.GetType(), ref right))
                if (!TryParseConst(left, right.GetType(), ref left))
                    if (right is IConvertible)
                        right = Convert.ChangeType(right, left.GetType());

            return left.Equals(right);
        }

        private static int Compare(object left, object right)
        {
            if (left.GetType() != right.GetType())
            {
                if (!TryParseConst(right, left.GetType(), ref right))
                    TryParseConst(left, right.GetType(), ref left);
            }

            int c;
            var comparable = left as IComparable;
            if (comparable != null)
            {
                var leftC = comparable;

                if (right is IConvertible)
                    right = Convert.ChangeType(right, comparable.GetType());

                c = leftC.CompareTo(right);
            }
            else
                throw new Exception("Cannot compare '" + left + "' and '" + left + "'");
            return c;
        }

        private static bool TryParseConst(object obj, Type ofType, ref object result)
        {
            var str = obj as string;
            if (str == null)
                return false;

            if (ofType == typeof(string))
            {
                result = obj;
                return true;
            }
            if (ofType == typeof(bool))
            {
                if (!TryParseBy<bool>(bool.TryParse, str, ref result))
                    return TryParseBy<int>(int.TryParse, str, ref result); // str might contain int 0 or 1
                return false;
            }
            if (ofType == typeof(int))
            {
                if (!TryParseBy<int>(int.TryParse, str, ref result))
                    if (!TryParseBy<bool>(bool.TryParse, str, ref result))
                        return true;
                return false;
            }
            if (ofType == typeof(long))
            {
                if (!TryParseBy<long>(long.TryParse, str, ref result))
                    if (!TryParseBy<bool>(bool.TryParse, str, ref result))
                        return true;
                return false;
            }
            if (ofType == typeof(double))
            {
                double d;
                if (!double.TryParse(str, out d))
                    if (!double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                        return false;
                result = d;
                return true;
            }
            if (ofType == typeof(decimal))
            {
                return TryParseBy<double>(double.TryParse, str, ref result);
            }
            if (ofType == typeof(Guid))
            {
                return TryParseBy<Guid>(Guid.TryParse, str, ref result);
            }
            if (ofType == typeof(DateTime))
            {
                DateTime dt;
                if (!DateTime.TryParse(str, out dt))
                    if (!DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        return false;
                result = dt;
                return true;
            }
            throw new Exception("Type not found: " + ofType);
        }

        private delegate bool TryParseByDelegate<T>(string input, out T value);

        private static bool TryParseBy<T>(TryParseByDelegate<T> action, string input, ref object result)
        {
            T value;
            bool success = action(input, out value);
            if (success)
                result = value;
            return success;
        }
    }
}