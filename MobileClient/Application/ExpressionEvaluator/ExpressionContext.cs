using BitMobile.Common.ExpressionEvaluator;

namespace BitMobile.Application.ExpressionEvaluator
{
    public static class ExpressionContext
    {
        public static IExpressionContext Current { get; private set; }

        public static void Init(IExpressionContext context)
        {
            Current = context;
        }
    }
}