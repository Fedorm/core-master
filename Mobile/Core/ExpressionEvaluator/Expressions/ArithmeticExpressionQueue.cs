using System;

namespace BitMobile.ExpressionEvaluator.Expressions
{
    class ArithmeticExpressionQueue : ExpressionQueue<decimal>
    {
        public const string ADDITION = "+";
        public const string SUBTRACTION = "-";
        public const string MULTIPLICATION = "*";
        public const string DIVISION = "/";

        bool _isMultiplicative;

        public ArithmeticExpressionQueue(ExpressionFactory factory, bool isMulitiplicative = false)
            : base(factory)
        {
            _isMultiplicative = isMulitiplicative;
        }

        public override decimal Evaluate(object root)
        {
            if (_first.Prefix != EMPTY)
                throw new Exception("Incorrect sign: " + _first.ToString());
            decimal result = _first.Evaluate(root);

            foreach (var current in _expressions)
            {
                switch (current.Prefix)
                {
                    case ADDITION:
                        {
                            result += current.Evaluate(root);
                        }
                        break;
                    case SUBTRACTION:
                        {
                            result -= current.Evaluate(root);
                        }
                        break;
                    case MULTIPLICATION:
                        {
                            result *= current.Evaluate(root);
                        }
                        break;
                    case DIVISION:
                        {
                            result /= current.Evaluate(root);
                        }
                        break;
                    default:
                        throw new Exception("Incorrect sign: " + current.ToString());
                }
            }
            return result;
        }

        public override bool IsExpressionEdge(string str, int i)
        {
            if (str.Length > i + 1)
            {
                string substring = str.Substring(i + 1, 1);
                if (_isMultiplicative)
                    return substring == MULTIPLICATION || substring == DIVISION;
                else
                    return substring == ADDITION || substring == SUBTRACTION;
            }
            return false;
        }

        public override void Handle(string expression)
        {
            IExpression<decimal> result;

            expression = expression.Trim();

            string prefix = GetPrefix(expression);
            string condition = expression;
            if (prefix != EMPTY)
                condition = expression.Substring(1).Trim();

            if (condition[0] == '(')
            {
                string bracketsBlock = condition.Substring(1, condition.Length - 2);

                ArithmeticExpressionQueue childBlock = new ArithmeticExpressionQueue(_factory);
                result = Builder.BuildBlockExpression<decimal>(bracketsBlock, childBlock);
            }
            else if (IsChildBlock(condition))
            {
                ArithmeticExpressionQueue childBlock = new ArithmeticExpressionQueue(_factory, true);
                result = Builder.BuildBlockExpression<decimal>(condition, childBlock);
            }
            else
                result = Builder.BuildValueExpression<decimal>(condition, _factory);
            
            Add(new ExpressionItem<decimal>(result, prefix));
        }

        bool IsChildBlock(string condition)
        {
            if (!_isMultiplicative)
                return condition.Contains(MULTIPLICATION) || condition.Contains(DIVISION);
            return false;
        }

        string GetPrefix(string expression)
        {
            string result;

            string substring = expression.Substring(0, 1);
            if (!_isMultiplicative)
            {
                if (substring == "+")
                    result = ADDITION;
                else if (substring == "-")
                    result = SUBTRACTION;
                else
                    result = EMPTY;
            }
            else
            {
                if (substring == "*")
                    result = MULTIPLICATION;
                else if (substring == "/")
                    result = DIVISION;
                else
                    result = EMPTY;
            }

            return result;
        }
    }
}
