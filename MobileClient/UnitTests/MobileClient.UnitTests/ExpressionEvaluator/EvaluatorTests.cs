using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.ValueStack;
using BitMobile.ExpressionEvaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace BitMobile.MobileClient.UnitTests.ExpressionEvaluator
{
    [TestClass]
    public class EvaluatorTests
    {
        [TestMethod]
        public void Evaluate_Null_ReturnsNull()
        {
            Evaluator evaluator = CreateEvaluator();

            object actual = evaluator.Evaluate("null");

            Assert.AreEqual(null, actual);
        }

        [TestMethod]
        public void Evaluate_SingleVariable()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "42" } });

            object actual = evaluator.Evaluate(" $value ");

            Assert.AreEqual("42", actual);
        }

        [TestMethod]
        public void Evaluate_VariableMember()
        {
            var stubObject = Substitute.For<IStubObject>();
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", stubObject } });
            stubObject.Name.Returns("M.Fauler");

            object actual = evaluator.Evaluate("$value.Name");

            Assert.AreEqual("M.Fauler", actual);
        }

        [TestMethod]
        public void Evaluate_VariableMembersQueue()
        {
            var stubObject = Substitute.For<IStubObject>();
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", stubObject } });
            stubObject.Obj.Returns(info =>
            {
                var stubSubObject = Substitute.For<IStubObject>();
                stubSubObject.Name.Returns("N.Virt");
                return stubSubObject;
            });

            object actual = evaluator.Evaluate("  $value.Obj.Name");

            Assert.AreEqual("N.Virt", actual);
        }

        [TestMethod]
        public void Evaluate_CallScriptSimple()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(controller: controller);
            controller.CallFunction("GetValue", Arg.Any<object[]>()).Returns("B.Meyer");

            object actual = evaluator.Evaluate(" $GetValue()");

            controller.Received(1).CallFunction("GetValue", Arg.Is<object[]>(o => o.Length == 0));
            Assert.AreEqual("B.Meyer", actual);
        }

        [TestMethod]
        public void Evaluate_CallScriptParams()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "42" } }, controller);

            controller.CallFunction("Get", Arg.Is<object[]>(o => o.Length == 0)).Returns("flower");
            controller.CallFunction("GetValue", Arg.Any<object[]>()).Returns("B.Meyer");

            object actual = evaluator.Evaluate(" $GetValue(hello, $value, $Get(), Null )");

            controller.Received(1).CallFunction("GetValue"
                , Arg.Is<object[]>(o => o.SequenceEqual(new object[] { "hello", "42", "flower", null })));
            Assert.AreEqual("B.Meyer", actual);
        }

        [TestMethod]
        public void Evaluate_CallVariableSimple()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(controller: controller);
            controller.CallVariable("value").Returns("A.Hejlsberg");

            object actual = evaluator.Evaluate("@value");

            Assert.AreEqual("A.Hejlsberg", actual);
        }

        [TestMethod]
        public void Evaluate_CallVariableMember()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(controller: controller);
            var member = Substitute.For<IStubObject>();
            controller.CallVariable("value").Returns(member);
            member.Name.Returns("A.Hejlsberg");

            object actual = evaluator.Evaluate(" @value.Name");

            Assert.AreEqual("A.Hejlsberg", actual);
        }

        [TestMethod]
        public void Evaluate_String()
        {
            Evaluator evaluator = CreateEvaluator();

            object actual = evaluator.Evaluate(" L.Torvalds ");

            Assert.AreEqual("L.Torvalds ", actual);
        }

        [TestMethod]
        public void Evaluate_StringWithQuotes()
        {
            Evaluator evaluator = CreateEvaluator();

            object actual = evaluator.Evaluate("  ' R.Stallman, '   ");

            Assert.AreEqual(" R.Stallman, ", actual);
        }

        [TestMethod]
        public void Evaluate_TranslationSimple()
        {
            Evaluator evaluator = CreateEvaluator(translations: new Dictionary<string, string> { { "ada", "A.Lovelace" } });

            object actual = evaluator.Evaluate("#ada#");

            Assert.AreEqual("A.Lovelace", actual);
        }

        [TestMethod]
        public void Evaluate_TranslationMultiple()
        {
            Evaluator evaluator = CreateEvaluator(
                translations: new Dictionary<string, string> { { "c", "C" }, { "b", "Babbage" } });

            object actual = evaluator.Evaluate(" #c#.#b#");

            Assert.AreEqual("C.Babbage", actual);
        }

        [TestMethod]
        public void Evaluate_TranslationComplicated()
        {
            Evaluator evaluator = CreateEvaluator(translations: new Dictionary<string, string>
            {
                { "bit", "Bit" }, { "byte", "Byte" }
            });

            object actual = evaluator.Evaluate("1 #byte# = 8 #bit#");

            Assert.AreEqual("1 Byte = 8 Bit", actual);
        }

        [TestMethod]
        public void Evaluate_SubexpressionSingle()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "42" } });

            object actual = evaluator.Evaluate(
                "Answer to the Ultimate 'Question' of Life, The Universe and Everything: { $value}!");

            Assert.AreEqual("Answer to the Ultimate 'Question' of Life, The Universe and Everything: 42!", actual);
        }

        [TestMethod]
        public void Evaluate_SubexpressionMultiple()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(controller: controller
                , translations: new Dictionary<string, string> { { "creed", "Rifleman's_Creed" } });
            controller.CallFunction("getWeapon", Arg.Any<object[]>()).Returns("rifle");

            object actual = evaluator.Evaluate(
                "This is my {$getWeapon() } my { $getWeapon()} is my best friend. #creed#");

            controller.Received(2).CallFunction("getWeapon", Arg.Is<object[]>(o => o.Length == 0));
            Assert.AreEqual("This is my rifle my rifle is my best friend. Rifleman's_Creed", actual);
        }

        [TestMethod]
        public void Evaluate_SubexpressionComplicated()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "name", "Alex" } }, controller);
            controller.CallFunction("getInfo", Arg.Any<object[]>()).Returns("some info");
            controller.CallFunction("getValue", Arg.Any<object[]>()).Returns("success");

            object actual = evaluator.Evaluate("$getValue( My name is { $name }, {$getInfo() })");

            controller.Received(1).CallFunction("getInfo", Arg.Is<object[]>(o => o.Length == 0));
            controller.Received(1).CallFunction("getValue"
                , Arg.Is<object[]>(o => o.SequenceEqual(new object[] { "My name is Alex", "some info" })));
            Assert.AreEqual("success", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EvaluateBooleanExpression_Empty()
        {
            Evaluator evaluator = CreateEvaluator();
            evaluator.EvaluateBooleanExpression("");
        }

        [TestMethod]
        public void EvaluateBooleanExpression_Constant()
        {
            Evaluator evaluator = CreateEvaluator();
            bool actual = evaluator.EvaluateBooleanExpression("TruE");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SingleValue()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", true } });
            bool actual = evaluator.EvaluateBooleanExpression("$value");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_Method()
        {
            var controller = Substitute.For<IExternalFunction>();
            Evaluator evaluator = CreateEvaluator(controller: controller);
            controller.CallFunction("HasValue", Arg.Any<object[]>()).Returns(false);
            bool actual = evaluator.EvaluateBooleanExpression("$HasValue(text)");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpression()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "42" } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value==42");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionWithCast()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", 42.5 } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value >=42.5 ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionWithCastVAriables()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", true }, { "value2", "true" } });
            bool actual = evaluator.EvaluateBooleanExpression("  $value1 ==   $value2");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionWithConstantCast()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", false } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value !=true ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionRawString()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "T.Cormen" } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value !=T.Cormen");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionRawStringWithQuotes()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", "B.Stroustrup" } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value == 'B.Stroustrup' ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_SimpleExpressionWithNull()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", null } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value == NulL ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_ComplicatedExpression()
        {
            var stubObject = Substitute.For<IStubObject>();
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value", stubObject } });
            stubObject.Name.Returns("J.Richter");
            bool actual = evaluator.EvaluateBooleanExpression("$value.Name != 'A.Troelsen'  ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_And()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", 42 }, { "value2", false } });
            bool actual = evaluator.EvaluateBooleanExpression("$value1 < 100 AND $value2");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_AndOld()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", 42 }, { "value2", false } });
            bool actual = evaluator.EvaluateBooleanExpression("$value1 < 100 && $value2");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_Or()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", false }, { "value2", 0 } });
            bool actual = evaluator.EvaluateBooleanExpression("$value1 OR$value2 == 0");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_OrOld()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", false }, { "value2", 0 } });
            bool actual = evaluator.EvaluateBooleanExpression("$value1 || $value2 == 0");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_Queue()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", true }, { "value2", 10 } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value1 AND $value2<0  ||$value2 == 10 ");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EvaluateBooleanExpression_ComplicatedQueue()
        {
            Evaluator evaluator = CreateEvaluator(new Dictionary<string, object> { { "value1", true }, { "value2", 10 } });
            bool actual = evaluator.EvaluateBooleanExpression(" $value1 && $value2<0  OR$value2 == 10 || False AND True");
            Assert.IsTrue(actual);
        }

        private static Evaluator CreateEvaluator(Dictionary<string, object> values = null, IExternalFunction controller = null, Dictionary<string, string> translations = null)
        {
            var valueStack = Substitute.For<IValueStack>();
            if (values != null)
                valueStack.Values.Returns(values);

            var translator = Substitute.For<ITranslator>();
            if (translations != null)
                translator.TranslateByKey(Arg.Any<string>()).Returns(info => translations[info.Arg<string>()]);

            var evaluator = new Evaluator(valueStack, translator);
            evaluator.SetController(controller ?? Substitute.For<IExternalFunction>());
            return evaluator;
        }

        public interface IStubObject
        {
            string Name { get; }
            IStubObject Obj { get; }
        }
    }
}
