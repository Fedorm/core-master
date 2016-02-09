using BitMobile.Common.Application;
using BitMobile.Common.ExpressionEvaluator;
using BitMobile.Common.ValueStack;

namespace BitMobile.ExpressionEvaluator
{
    public class ExpressionContext : IExpressionContext
    {
        private readonly IApplicationContext _applicationContext;

        public ExpressionContext(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public IEvaluator CreateEvaluator(IValueStack valueStack)
        {
            return new Evaluator(valueStack, _applicationContext.Dal);
        }
    }
}
