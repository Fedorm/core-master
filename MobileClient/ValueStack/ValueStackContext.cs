using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ExpressionEvaluator;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack.Stack;

namespace BitMobile.ValueStack
{
    public class ValueStackContext : IValueStackContext
    {
        private readonly IDbRefFactory _dbRefFactory;
        private readonly IExpressionContext _expressionContext;

        public ValueStackContext(IDbRefFactory dbRefFactory, IExpressionContext expressionContext)
        {
            _dbRefFactory = dbRefFactory;
            _expressionContext = expressionContext;
        }

        public IValueStack CreateValueStack(IExceptionHandler exceptionHandler)
        {
            return new Stack.ValueStack(exceptionHandler, _expressionContext);
        }

        public ICustomDictionary CreateDictionary()
        {
            return new CustomDictionary();
        }

        public IIteratorStatus CreateIteratorStatus()
        {
            return new IteratorStatus();
        }

        public ICommonData CreateCommonData(string os)
        {
            return new CommonData.CommonData(os, _dbRefFactory);
        }
    }
}
