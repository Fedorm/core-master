using System;

namespace BitMobile.ExpressionEvaluator
{
    class ExpressionParser : Parser
    {
        public ExpressionParser(Executor executor)
            : base(executor)
        { }

        public bool? ParseBooleanExpression(string expression, ref int index)
        {
            bool result = true;
            var bindingOperator = Operator.And;

            do
            {
                object left = ParseValue(expression, ref index, NullCharsExpression);
                Operator o = ParseOperator(expression, ref index);

                bool current;
                if (o > Operator.Or)
                {
                    object right = ParseValue(expression, ref index, NullCharsExpression);
                    current = Executor.ExecuteBoolean(left, right, o);

                    o = ParseOperator(expression, ref index);
                }
                else
                    current = Executor.ConvertToBoolean(left);

                switch (bindingOperator)
                {
                    case Operator.None:
                    case Operator.And:
                        result &= current;
                        break;
                    case Operator.Or:
                        result |= current;
                        break;
                    default:
                        throw new Exception("Unexpected operator " + o + " in " + expression);
                }

                bindingOperator = o;
                SkipWhiteSpace(expression, ref index);

            } while (index < expression.Length);

            return result;
        }

        private Operator ParseOperator(string expression, ref int index)
        {
            SkipWhiteSpace(expression, ref index);

            if (index >= expression.Length)
                return Operator.None;

            switch (expression[index])
            {
                case '&':
                case 'A':
                    if (index + 1 < expression.Length && expression[index + 1] == '&')
                    {
                        index += 2;
                        return Operator.And;
                    }
                    if (index + 2 < expression.Length && expression[index + 1] == 'N' && expression[index + 2] == 'D')
                    {
                        index += 3;
                        return Operator.And;
                    }
                    break;
                case '|':
                case 'O':
                    if (index + 1 < expression.Length && (expression[index + 1] == 'R' || expression[index + 1] == '|'))
                    {
                        index += 2;
                        return Operator.Or;
                    }
                    break;
                case '=':
                    if (index + 1 < expression.Length && expression[index + 1] == '=')
                    {
                        index += 2;
                        return Operator.Equality;
                    }
                    break;
                case '!':
                    if (index + 1 < expression.Length && expression[index + 1] == '=')
                    {
                        index += 2;
                        return Operator.Inequality;
                    }
                    break;
                case '<':
                    if (index + 1 < expression.Length && expression[index + 1] == '=')
                    {
                        index += 2;
                        return Operator.LessOrEqual;
                    }
                    index++;
                    return Operator.Less;
                case '>':
                    if (index + 1 < expression.Length && expression[index + 1] == '=')
                    {
                        index += 2;
                        return Operator.GreaterOrEqual;
                    }
                    index++;
                    return Operator.Greater;
            }
            return Operator.None;
        }

        public enum Operator
        {
            None,
            And,
            Or,
            Equality,
            Inequality,
            Less,
            Greater,
            LessOrEqual,
            GreaterOrEqual
        }
    }
}