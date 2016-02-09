using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitMobile.Common.Develop;

namespace BitMobile.ExpressionEvaluator
{
    class Parser
    {
        private static readonly char[] NullCharsNone = new char[0];
        private static readonly char[] NullCharsString = { '\'' };
        private static readonly char[] NullCharsArgs = { ',', ')' };
        private static readonly char[] NullCharsFormat = { '}' };
        internal static readonly char[] NullCharsExpression = { '=', '!', '>', '<', ' ', '&', '|' };

        protected readonly Executor Executor;

        protected Parser(Executor executor)
        {
            Executor = executor;
        }

        protected void SkipWhiteSpace(string expression, ref int index)
        {
            while (index < expression.Length)
            {
                char c = expression[index];
                if (c != ' ')
                    break;
                index++;
            }
        }

        public object ParseValue(string expression, ref int index, char[] nullChars = null)
        {
            if (nullChars == null) nullChars = NullCharsNone;
            if (index >= expression.Length)
                return null;

            SkipWhiteSpace(expression, ref index);

            char c = expression[index];

            if (c == '$')
                return ParseMethodOrVariable(expression, ref index);

            if (c == '@')
                return ParseControllerVariable(expression, ref index);

            if (c == '\'')
                return ParseString(expression, ref index);

            return ParseRawString(expression, ref index, nullChars);
        }

        private object ParseMethodOrVariable(string expression, ref int index)
        {
            Assert.AreEqual(expression[index], '$');
            int start = index + 1;

            while (++index < expression.Length)
                if (!IsIdentifierPart(expression[index]))
                    break;

            string identifier = expression.Substring(start, index - start);
            if (index < expression.Length)
            {
                // method, like " $GetValue(something)"
                if (expression[index] == '(')
                {
                    object[] args = ParseArgs(expression, ref index);
                    object result = Executor.ExecuteFunction(identifier, args);
                    Assert.True(index >= expression.Length || expression[index] == ')');
                    index++;
                    return result;
                }

                //  variable with members, like "$value.Name"
                if (expression[index] == '.')
                {
                    object root;
                    if (Executor.TryGetVariable(identifier, out root))
                        return ParseMember(expression, ref index, root);
                    return null;
                }
                // If expression is $value~lol, we will return $value and ignore ~lol
            }

            // single variable, like "$value"
            object value;
            if (Executor.TryGetVariable(identifier, out value))
                return value;
            return null;
        }

        private object ParseMember(string expression, ref int index, object root)
        {
            Assert.AreEqual(expression[index], '.');
            index++;

            SkipWhiteSpace(expression, ref index);

            int start = index;
            do
            {
                if (!IsIdentifierPart(expression[index]))
                    break;
            } while (++index < expression.Length);

            if (index == start)
                return root; // for example: " $value.Name " - at the end; "{$value.Name}" - at '}'

            var identifier = expression.Substring(start, index - start);
            object result = Executor.ExecuteMember(root, identifier);

            if (index < expression.Length && expression[index] == '.')
                return ParseMember(expression, ref index, result);

            return result;
        }

        private object[] ParseArgs(string expression, ref int index)
        {
            Assert.AreEqual(expression[index], '(');
            index++;

            var args = new List<object>();
            while (index < expression.Length)
            {
                if (expression[index] == ')')
                    break;

                args.Add(ParseValue(expression, ref index, NullCharsArgs));

                if (expression[index] == ',')
                    index++;
            }

            return args.ToArray();
        }

        private object ParseControllerVariable(string expression, ref int index)
        {
            Assert.AreEqual(expression[index], '@');
            int start = index + 1;

            while (++index < expression.Length)
                if (!IsIdentifierPart(expression[index]))
                    break;

            object variable = Executor.GetControllerVariable(expression.Substring(start, index - start));

            // variable with members
            if (index < expression.Length && expression[index] == '.')
                return ParseMember(expression, ref index, variable);

            return variable;
        }

        private string ParseString(string expression, ref int index)
        {
            Assert.AreEqual(expression[index], '\'');
            index++;
            string str = ParseRawString(expression, ref index, NullCharsString);
            Assert.AreEqual(expression[index], '\'');
            index++;
            return str;
        }

        private string ParseRawString(string expression, ref int index, char[] nullChars)
        {
            var builder = new StringBuilder();
            int start = index;

            bool nullPossible = expression[index] == 'n' || expression[index] == 'N';

            while (index < expression.Length)
            {
                char c = expression[index];

                if (c == '#')
                {
                    builder.Append(expression, start, index - start);
                    string translation = ParseTranslation(expression, ref index);
                    builder.Append(translation);
                    Assert.AreEqual(expression[index], '#');
                    start = index + 1;
                }
                else if (c == '{')
                {
                    builder.Append(expression, start, index - start);
                    index++;
                    object value = ParseValue(expression, ref index, NullCharsFormat);
                    builder.Append(value);
                    while (index < expression.Length && expression[index] != '}')
                        index++;
                    start = index + 1;
                }
                else if (nullChars.Contains(c))
                    break;

                index++;
            }
            builder.Append(expression, start, index - start);
            string result = builder.ToString();

            if (nullPossible && result.Trim().Equals("null", StringComparison.InvariantCultureIgnoreCase))
                return null;
            return result;
        }

        private string ParseTranslation(string expression, ref int index)
        {
            Assert.AreEqual(expression[index], '#');
            int start = index;

            while (++index < expression.Length)
                if (expression[index] == '#')
                    break;

            string key = expression.Substring(start + 1, index - start - 1);
            return Executor.TranslateByKey(key);
        }

        private static bool IsIdentifierPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}