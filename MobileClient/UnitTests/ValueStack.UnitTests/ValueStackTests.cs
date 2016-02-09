using BitMobile.Common.Application;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.ExpressionEvaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace BitMobile.MobileClient.UnitTests
{
    [TestClass]
    public class ValueStackTests
    {
        [TestMethod]
        public void Evaluate_OneWordSimpleExpression_ReturnsValueFromStack()
        {
            var applicationContext = Substitute.For<IApplicationContext>();
            applicationContext.Dal.Returns(Substitute.For<IDal>());
            var valueStack = new ValueStack.Stack.ValueStack(Substitute.For<IExceptionHandler>()
                , Substitute.For<IExpressionContext>(), applicationContext);
            
            applicationContext.Dal.TranslateString("$value").Returns("$value");

            valueStack.Values.Add("value", 42);

            object actual = valueStack.Evaluate("$value");

            Assert.AreEqual(42, actual);
        }

    }
}
