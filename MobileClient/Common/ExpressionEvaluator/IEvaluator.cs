using System;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.ExpressionEvaluator
{
    public interface IEvaluator
    {
        void SetController(IExternalFunction controller);
        object Evaluate(string expression, Type type = null);
        bool EvaluateBooleanExpression(string expression);
    }
}