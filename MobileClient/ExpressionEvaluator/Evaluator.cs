using System;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.ExpressionEvaluator;
using BitMobile.Common.ValueStack;

namespace BitMobile.ExpressionEvaluator
{
    class Evaluator : IEvaluator
    {
        private readonly Executor _executor;
        private readonly ExpressionParser _parser;

        public Evaluator(IValueStack valueStack, ITranslator translator)
        {
            _executor = new Executor(valueStack, translator);
            _parser = new ExpressionParser(_executor);
        }

        public void SetController(IExternalFunction controller)
        {
            _executor.SetController(controller);
        }

        public object Evaluate(string expression, Type type = null)
        {
            try
            {
                int index = 0;
                object result = _parser.ParseValue(expression, ref index);

                if (result != null && type != null)
                {
                    Type resultType = result.GetType();
                    if (resultType != type && !type.IsAssignableFrom(resultType))
                        result = Convert.ChangeType(result, type);
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Parsing error: '" + expression + "'", e);
            }
        }

        public bool EvaluateBooleanExpression(string expression)
        {
            try
            {
                int index = 0;
                bool? result = _parser.ParseBooleanExpression(expression, ref index);
                if (result != null)
                    return result.Value;
                return false;
            }
            catch (Exception e)
            {
                throw new Exception("Parsing error: '" + expression + "'", e);
            }
        }
    }
}
