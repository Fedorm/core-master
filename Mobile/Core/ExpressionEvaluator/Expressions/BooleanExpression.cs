using System;
using System.Collections;
using System.Diagnostics;

namespace BitMobile.ExpressionEvaluator.Expressions
{
    [DebuggerDisplay("{DebugString}")]
    class BooleanExpression : IExpression<bool>
    {
        public readonly string DebugString;

        public const string EQUALITY = "==";
        public const string INEQUALITY = "!=";
        public const string LESS_THAN_OR_EQUAL = "<=";
        public const string GREATER_THAN_OR_EQUAL = ">=";
        public const string LESS_THAN = "<";
        public const string GREATER_THAN = ">";
        public const string IN = " IN ";
        public const string METHOD_IVOKE = "()";

        IExpression<object> _left;
        IExpression<object> _right;
        string _operator;

        public BooleanExpression(IExpression<object> left
            , IExpression<object> right
            , string oper
            , string expressionString)
        {
            DebugString = expressionString;

            _left = left;
            _right = right;
            _operator = oper;
        }

        public BooleanExpression(IExpression<object> left, string expressionString)
        {
            _left = left;
            _operator = METHOD_IVOKE;

            DebugString = expressionString;
        }

        public bool Evaluate(object root)
        {
            bool result;

            object leftValue = _left.Evaluate(root);

            if (_operator != METHOD_IVOKE)
            {
                object rightValue = _right.Evaluate(root);

                if (leftValue != null)
                    switch (_operator)
                    {
                        case EQUALITY:
                            result = Equality(leftValue, rightValue);
                            break;
                        case INEQUALITY:
                            result = !Equality(leftValue, rightValue);
                            break;
                        case LESS_THAN_OR_EQUAL:
                            result = Compare(leftValue, rightValue) <= 0;
                            break;
                        case GREATER_THAN_OR_EQUAL:
                            result = Compare(leftValue, rightValue) >= 0;
                            break;
                        case LESS_THAN:
                            result = Compare(leftValue, rightValue) < 0;
                            break;
                        case GREATER_THAN:
                            result = Compare(leftValue, rightValue) > 0;
                            break;
                        case IN:
                            {
                                IEnumerable rightCollection = rightValue as IEnumerable;
                                if (rightValue != null)
                                    result = Helper.In(leftValue, rightCollection);
                                else
                                    result = Equality(leftValue, rightValue);                               
                            }
                            break;
                        default:
                            throw new NotSupportedException(
                                "Incorrect operator: " + _operator);
                    }
                else
                    switch (_operator)
                    {
                        case EQUALITY:
                            result = rightValue == null;
                            break;
                        case INEQUALITY:
                            result = rightValue != null;
                            break;
                        default:
                            throw new NotSupportedException(
                                "Incorrect operator, if the left operand is null: " + _operator);
                    }
            }
            else
                result = (bool)leftValue;

            return result;
        }

        int Compare(object left, object right)
        {
            int c;
            if (left is IComparable)
            {
                IComparable leftC = (IComparable)left;

                if (right is IConvertible)
                    right = (IComparable)Convert.ChangeType(right, left.GetType());

                c = leftC.CompareTo(right);
            }
            else
                throw new Exception("Cannot compare string '" + DebugString
                    + "'. Parameters: " + left.ToString() + "; " + _right.ToString());
            return c;
        }

        bool Equality(object left, object right)
        {
            if (object.ReferenceEquals(left, right))
                return true;

            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            if (left.GetType().Equals(right.GetType()))
                return left.Equals(right);

            if (right is IConvertible)
                right = (IComparable)Convert.ChangeType(right, left.GetType());

            return left.Equals(right);
        }

        public override string ToString()
        {
            return DebugString;
        }
    }

}
