using BitMobile.ExpressionEvaluator.Expressions;
using System;
using System.Collections.Generic;

namespace BitMobile.ExpressionEvaluator
{
    public class ExpressionFactory
    {
        public ExpressionFactory(IDictionary<string, object> valueStack, Type baseType, IDictionary<string, object> parameters = null)
        {
            BaseType = baseType;
            ValueStack = valueStack;

            Parameters = new Dictionary<string, object>();

            if (parameters != null)
                foreach (var pair in parameters)
                    Parameters.Add(pair.Key, pair.Value);
        }

        public ExpressionFactory(IDictionary<string, object> valueStack, IDictionary<string, object> parameters = null)
            : this(valueStack, typeof(object), parameters)
        {
        }

        public ExpressionFactory(object offlineContext, Type baseType, IDictionary<string, object> parameters = null)
            : this(new Dictionary<string, object>(), baseType, parameters)
        {
            // TODO: Костыль для поддержки AsObject
            ValueStack.Add("dao", offlineContext);
        }

        public IDictionary<string, object> ValueStack { get; private set; }

        public Dictionary<string, object> Parameters { get; private set; }

        public Type BaseType { get; private set; }

        public void AddParameter(string key, object value)
        {
            Parameters.Add(key, value);
        }

        public Func<object, bool> BuildLogicalExpression(string expression)
        {
            LogicalExpressionQueue block = new LogicalExpressionQueue(this);

            IExpression<bool> exp = Builder.BuildBlockExpression<bool>(expression, block);

            return new Func<object, bool>(exp.Evaluate);
        }

        public Func<object, decimal> BuildArithmeticExpression(string expression)
        {
            ArithmeticExpressionQueue block = new ArithmeticExpressionQueue(this);

            IExpression<decimal> exp = Builder.BuildBlockExpression<decimal>(expression, block);

            return new Func<object, decimal>(exp.Evaluate);
        }

        public Func<object, object> BuildValueExpression(string expression)
        {
            IExpression<object> exp = Builder.BuildValueExpression<object>(expression, this);

            return new Func<object, object>(exp.Evaluate);
        }
    }
}
