using System;

namespace BitMobile.Common.ExpressionEvaluator
{
    public interface IExpressionFactory
    {
        Func<object, bool> BuildLogicalExpression(string expression);
    }
}
