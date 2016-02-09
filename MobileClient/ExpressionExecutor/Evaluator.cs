using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ExpressionExecutor
{
    class Evaluator
    {
        public object EvaluateEx(string expression, Type type = null, bool canString = true)
        {
            int index = 0;
            return ParseValue(expression, ref index);
        }

        private object ParseValue(string expression, ref int index)
        {
            char c = expression[index];

            if (c == '$')
                return ParseMethodOrVariable(expression, ref index);

            if (c == '@')
                return ParseControllerVariable(expression, index);

            if (c == '#')
                return ParseTranslation(expression, index);

            if (c == '{' || char.IsLetterOrDigit(c))
                return ParseRawString(expression, index);

            if (c != ' ')
                throw new Exception("Cannot parse expression: " + expression + "Error in: " + index);

            return ParseValue(expression, ref index);
        }

        private object ParseMethodOrVariable(string expression, ref int index)
        {
            int start = index + 1;

            while (++index < expression.Length)
            {
                if (!char.IsLetterOrDigit(expression[index]))
                    break;
            }
            int length = index - start;

            if (index < expression.Length && expression[index] == '(')
            {
                object[] args = ParseArgs(expression, ref index);
                return null;// call function
            }

            return null;// exec variable
            // If expression is $value~lol, we will return $value and ignore ~lol
        }

        private object[] ParseArgs(string expression, ref int index)
        {
            var args = new List<object>();
            index++; // check expression[index] == '('

            while (index < expression.Length)
            {
                if (expression[index] == ')')
                    break;

                args.Add(ParseValue(expression, ref index));

                if (expression[index] == ',')
                    index++;
            }

            return args.ToArray();
        }

        private object ParseControllerVariable(string expression, int index)
        {
            int start = index + 1;

            while (++index < expression.Length)
            {
                if (!char.IsLetterOrDigit(expression[index]))
                    break;
            }
            int length = index - start;
            return null; // call from controller
        }

        private string ParseRawString(string expression, int index)
        {
            var builder = new StringBuilder();

            int start = index;
            while (index < expression.Length)
            {
                char c = expression[index];
                if (c == '#')
                {
                    builder.Append(expression, start, index - start);
                    string translation = ParseTranslation(expression, index);
                    builder.Append(translation);
                    index++; // chack expression[index] == '}'
                    start = index;
                }
                else if (c == '{')
                {
                    builder.Append(expression, start, index - start);
                    index++;
                    object value = ParseValue(expression, ref index);
                    builder.Append(value);
                    index++; // chack expression[index] == '}'
                    start = index;
                }
                index++;
            }
            builder.Append(expression, start, index - start);
            return builder.ToString();
        }

        private string ParseTranslation(string expression, int index)
        {
            int start = index;
            while (++index < expression.Length)
            {
                if (expression[index] == '#')
                    break;
            }

            return ""; // get from translation
        }
    }
}
