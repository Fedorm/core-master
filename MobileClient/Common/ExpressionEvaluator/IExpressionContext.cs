using BitMobile.Common.ValueStack;

namespace BitMobile.Common.ExpressionEvaluator
{
    public interface IExpressionContext
    {
        IEvaluator CreateEvaluator(IValueStack valueStack);
    }
}
