using System;
using System.Linq;

namespace BitMobile.ExpressionEvaluator.Expressions
{
    class LogicalExpressionQueue : ExpressionQueue<bool>
    {
        public const string AND = "&&";
        public const string OR = "||";

        public LogicalExpressionQueue(ExpressionFactory factory)
            : base(factory)
        {
        }

        public override bool Evaluate(object root)
        {
            if (_first.Prefix != EMPTY)
                throw new Exception("Incorrect sign: " + _first.ToString());
            bool result = _first.Evaluate(root);

            foreach (var current in _expressions)
            {
                switch (current.Prefix)
                {
                    case AND:
                        result &= current.Evaluate(root);
                        break;
                    case OR:
                        result |= current.Evaluate(root);
                        break;
                    default:
                        throw new Exception("Incorrect sign: " + current.ToString());
                }

            }
            return result;
        }

        public override void Handle(string expression)
        {
            IExpression<bool> result;

            expression = expression.Trim();

            string prefix = GetPrefix(expression);
            string condition = expression;
            if (prefix != EMPTY)
                condition = expression.Substring(2).Trim();

            if (condition[0] == '(')
            {
                string bracketsBlock = condition.Substring(1, condition.Length - 2);

                LogicalExpressionQueue childBlock = new LogicalExpressionQueue(_factory);
                result = Builder.BuildBlockExpression<bool>(bracketsBlock, childBlock);
            }
            else
            {
                string oper;
                if (TryFindOperator(condition, out oper))
                {
                    string[] strings = condition.Split(
                        new string[] { oper }, StringSplitOptions.RemoveEmptyEntries);

                    IExpression<object> left = Builder.BuildValueExpression<object>(strings[0].Trim(), _factory);

                    IExpression<object> right = Builder.BuildValueExpression<object>(strings[1].Trim(), _factory);

                    result = new BooleanExpression(left, right, oper, expression);
                }
                else
                    result = Builder.BuildValueExpression<bool>(condition, _factory);
            }

            Add(new ExpressionItem<bool>(result, prefix));
        }

        public override bool IsExpressionEdge(string str, int i)
        {
            if (str.Length > i + 2)
            {
                string substring = str.Substring(i + 1, 2);
                return substring == AND || substring == OR;
            }
            return false;
        }

        string GetPrefix(string expression)
        {
            string result = EMPTY;

            string prefix = expression.Substring(0, 2);
            if (prefix == AND || prefix == OR)
                result = prefix;

            return result;
        }

        bool TryFindOperator(string condition, out string oper)
        {
            oper = null;

            if (condition.Contains(BooleanExpression.EQUALITY))
                oper = BooleanExpression.EQUALITY;
            else if (condition.Contains(BooleanExpression.INEQUALITY))
                oper = BooleanExpression.INEQUALITY;
            else if (condition.Contains(BooleanExpression.LESS_THAN_OR_EQUAL))
                oper = BooleanExpression.LESS_THAN_OR_EQUAL;
            else if (condition.Contains(BooleanExpression.GREATER_THAN_OR_EQUAL))
                oper = BooleanExpression.GREATER_THAN_OR_EQUAL;
            else if (condition.Contains(BooleanExpression.LESS_THAN))
                oper = BooleanExpression.LESS_THAN;
            else if (condition.Contains(BooleanExpression.GREATER_THAN))
                oper = BooleanExpression.GREATER_THAN;
            else if (condition.Contains(BooleanExpression.IN))
                oper = BooleanExpression.IN;
            else
                return false;

            return true;
        }
    }
}
