using Jint;
using Jint.Expressions;
using Jint.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace MobileClient.Tests.ScriptEngine
{
    [TestClass]
    public class JsGlobalTests
    {
        JsGlobal _target;
        PrivateObject _accessor;

        [TestInitialize]
        public void Setup()
        {
            _target = new JsGlobal(new StubVisitor(), Jint.Options.Strict);
            _accessor = new PrivateObject(_target, new PrivateType(typeof(JsGlobal)));
        }

        #region Global functions

        [TestMethod]
        public void Validate_NumericMaskInputInt_ReturnTrue()
        {
            var actual = _target.Validate(GetStubs("42", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(true, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskInputFloat_ReturnTrue()
        {
            var actual = _target.Validate(GetStubs("42.120", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(true, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskInputDots_ReturnFalse()
        {
            var actual = _target.Validate(GetStubs("42..4", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(false, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskInputWords_ReturnFalse()
        {
            var actual = _target.Validate(GetStubs("42abab", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(false, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskInputWhiteSpace_ReturnFalse()
        {
            var actual = _target.Validate(GetStubs("    42    ", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(false, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskMultiline_ReturnFalse()
        {
            var actual = _target.Validate(GetStubs(@"
42
  ", @"\-?\d+(\.\d{0,})?"));
            Assert.AreEqual(false, actual.Value);
        }

        [TestMethod]
        public void Validate_NumericMaskInputEmpty_ReturnFalse()
        {
            var actual = _target.Validate(GetStubs("", @"^\-?\d+(\.\d{0,})?$"));
            Assert.AreEqual(false, actual.Value);
        }

        #endregion

        #region Functions for working with String type values

        [TestMethod]
        public void Title_CrazyInput_ReturnsNormal()
        {
            var actual = _target.Title(GetStubs("hElLo 1 ФсЕм ПРЕВЕД в этом ЧаТиКе!!!1 !!!"));
            Assert.AreEqual("Hello 1 Фсем Превед В Этом Чатике!!!1 !!!", actual.Value);
        }

        [TestMethod]
        public void Title_Empty_ReturnsEmpty()
        {
            var actual = _target.Title(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrOccurrenceCount_EnglishInput_ReturnsResult()
        {
            var actual = _target.StrOccurrenceCount(GetStubs("Hello hELLO hell ello hElLo hello!", "hello"));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void StrOccurrenceCount_RussianInput_ReturnsResult()
        {
            var actual = _target.StrOccurrenceCount(GetStubs("Привет привет пппривет пРиВеТ пр1ивет приветпри вет!", "привет"));
            Assert.AreEqual(3.0, actual.Value);
        }

        [TestMethod]
        public void StrOccurrenceCount_Empty_ReturnsZero()
        {
            var actual = _target.StrOccurrenceCount(GetStubs(string.Empty, "empty"));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void StrGetLine_MultiLine_ReturnsLine()
        {
            var actual = _target.StrGetLine(GetStubs(
@"Hello
      World          
!!!!!
111111"
, 2));
            Assert.AreEqual("      World          ", actual.Value);
        }

        [TestMethod]
        public void StrGetLine_OutOfBounds_ReturnsEmpty()
        {
            var actual = _target.StrGetLine(GetStubs("single line", int.MaxValue));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrGetLine_Zero_ReturnsEmpty()
        {
            var actual = _target.StrGetLine(GetStubs("single line", 0));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrGetLine_Empty_ReturnsEmpty()
        {
            var actual = _target.StrGetLine(GetStubs(string.Empty, 1));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrLineCount_MultiLine_ReturnsCount()
        {
            var actual = _target.StrLineCount(GetStubs(@"Hello
World
!!!!"));
            Assert.AreEqual(3.0, actual.Value);
        }

        [TestMethod]
        public void StrLineCount_SingleLine_ReturnsOne()
        {
            var actual = _target.StrLineCount(GetStubs("Hello World!!!"));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void StrLineCount_Empty_ReturnsOne()
        {
            var actual = _target.StrLineCount(GetStubs(string.Empty));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void StrReplace_Default_ReturnsModifiedString()
        {
            var actual = _target.StrReplace(GetStubs("Hello hello hel lo hellohell hello! he2llo", "hello", "bye"));
            Assert.AreEqual("Hello bye hel lo byehell bye! he2llo", actual.Value);
        }

        [TestMethod]
        public void StrReplace_SpaceInSearch_ReturnsModifiedString()
        {
            var actual = _target.StrReplace(GetStubs(" Fake   string   ! AAA, it is  Fake   string   !", " Fake   string   ", "Opilkin"));
            Assert.AreEqual("Opilkin! AAA, it is Opilkin!", actual.Value);
        }

        [TestMethod]
        public void StrReplace_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.StrReplace(GetStubs(string.Empty, " Fake   string   ", "Opilkin"));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrReplace_EmptySearch_ReturnsSourceString()
        {
            var actual = _target.StrReplace(GetStubs("Hello hello hel lo hellohell hello! he2llo", string.Empty, "bye"));
            Assert.AreEqual("Hello hello hel lo hellohell hello! he2llo", actual.Value);
        }

        [TestMethod]
        public void StrReplace_EmptyReplace_ReturnsModifiedString()
        {
            var actual = _target.StrReplace(GetStubs("Hello hello hel lo hellohell hello! he2llo", "hello", string.Empty));
            Assert.AreEqual("Hello  hel lo hell ! he2llo", actual.Value);
        }

        [TestMethod]
        public void IsBlankString_NotEmpty_ReturnsFalse()
        {
            var actual = _target.IsBlankString(GetStubs("Hello world!!!"));
            Assert.AreEqual(false, actual.Value);
        }

        [TestMethod]
        public void IsBlankString_EmptyLine_ReturnsTrue()
        {
            var actual = _target.IsBlankString(GetStubs(string.Empty));
            Assert.AreEqual(true, actual.Value);
        }

        [TestMethod]
        public void IsBlankString_InsignificantCharacters_ReturnsTrue()
        {
            var actual = _target.IsBlankString(GetStubs(" \t\v\r\n\f "));
            Assert.AreEqual(true, actual.Value);
        }

        [TestMethod]
        public void CharCode_English_ReturnsCharcode()
        {
            var actual = _target.CharCode(GetStubs("Hello world!!!", 7));
            Assert.AreEqual(119.0, actual.Value);
        }

        [TestMethod]
        public void CharCode_Russian_ReturnsCharcode()
        {
            var actual = _target.CharCode(GetStubs("Привет мир!!!", 6));
            Assert.AreEqual(1090.0, actual.Value);
        }

        [TestMethod]
        public void CharCode_OutOfBounds_ReturnsZero()
        {
            var actual = _target.CharCode(GetStubs("Hello world!!!", int.MaxValue));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void CharCode_WithoutNumber_ReturnsCharcode()
        {
            var actual = _target.CharCode(GetStubs("Hello world!!!"));
            Assert.AreEqual(72.0, actual.Value);
        }

        [TestMethod]
        public void CharCode_EmptyLine_ReturnsZero()
        {
            var actual = _target.CharCode(GetStubs(string.Empty, 0));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void Char_English_ReturnsChar()
        {
            var actual = _target.Char(GetStubs(65));
            Assert.AreEqual("A", actual.Value);
        }

        [TestMethod]
        public void Char_Russian_ReturnsChar()
        {
            var actual = _target.Char(GetStubs(1040));
            Assert.AreEqual("А", actual.Value);
        }

        [TestMethod]
        public void Char_OutOfBounds_ReturnsEmpty()
        {
            var actual = _target.Char(GetStubs(int.MaxValue));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Lower_Default_ReturnsString()
        {
            var actual = _target.Lower(GetStubs("Hello World!!!111"));
            Assert.AreEqual("hello world!!!111", actual.Value);
        }

        [TestMethod]
        public void Lower_Empty_ReturnsEmpty()
        {
            var actual = _target.Lower(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Upper_Default_ReturnsString()
        {
            var actual = _target.Upper(GetStubs("Hello World!!!111"));
            Assert.AreEqual("HELLO WORLD!!!111", actual.Value);
        }

        [TestMethod]
        public void Upper_Empty_ReturnsEmpty()
        {
            var actual = _target.Upper(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Find_Default_ReturnsIndex()
        {
            var actual = _target.Find(GetStubs("Hello World!1!111", " World!1"));
            Assert.AreEqual(6.0, actual.Value);
        }

        [TestMethod]
        public void Find_DoesntContain_ReturnsZero()
        {
            var actual = _target.Find(GetStubs("Hello World!1!111", " FaKe"));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void Find_EmptyLine_ReturnsZero()
        {
            var actual = _target.Find(GetStubs(string.Empty, " World!1"));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void Find_EmptySearch_ReturnsOne()
        {
            var actual = _target.Find(GetStubs("Hello World!1!111", string.Empty));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void Find_EmptyBoth_ReturnsOne()
        {
            var actual = _target.Find(GetStubs(string.Empty, string.Empty));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void Mid_Default_ReturnsSubstring()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", 6, 7));
            Assert.AreEqual(" World!", actual.Value);
        }

        [TestMethod]
        public void Mid_OutOfBoundsInitial_ReturnsEmpty()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", int.MaxValue, 5));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Mid_ZeroInitial_ReturnsSubstring()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", 0, 5));
            Assert.AreEqual("Hello", actual.Value);
        }

        [TestMethod]
        public void Mid_OutOfBoundsCount_ReturnsSubstring()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", 6, int.MaxValue));
            Assert.AreEqual(" World!!!111", actual.Value);
        }

        [TestMethod]
        public void Mid_ZeroCount_ReturnsEmpty()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", 1, 0));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Mid_WithoutCount_ReturnsSubstring()
        {
            var actual = _target.Mid(GetStubs("Hello World!!!111", 1));
            Assert.AreEqual("Hello World!!!111", actual.Value);
        }

        [TestMethod]
        public void Mid_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.Mid(GetStubs(string.Empty, 1, 1));
            Assert.AreEqual(string.Empty, actual.Value);
        }


        [TestMethod]
        public void Right_Default_ReturnsSubstring()
        {
            var actual = _target.Right(GetStubs("Hello World!!!111", 11));
            Assert.AreEqual("World!!!111", actual.Value);
        }

        [TestMethod]
        public void Right_OutOfBounds_ReturnsSourceString()
        {
            var actual = _target.Right(GetStubs("Hello World!!!111", int.MaxValue));
            Assert.AreEqual("Hello World!!!111", actual.Value);
        }

        [TestMethod]
        public void Right_Zero_ReturnsEmpty()
        {
            var actual = _target.Right(GetStubs("Hello World!!!111", 0));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Right_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.Right(GetStubs(string.Empty, 1));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Left_Default_ReturnsSubstring()
        {
            var actual = _target.Left(GetStubs("Hello World!!!111", 6));
            Assert.AreEqual("Hello ", actual.Value);
        }

        [TestMethod]
        public void Left_OutOfBounds_ReturnsSourceString()
        {
            var actual = _target.Left(GetStubs("Hello World!!!111", int.MaxValue));
            Assert.AreEqual("Hello World!!!111", actual.Value);
        }

        [TestMethod]
        public void Left_Zero_ReturnsEmpty()
        {
            var actual = _target.Left(GetStubs("Hello World!!!111", 0));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void Left_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.Left(GetStubs(string.Empty, 1));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimAll_Default_ReturnsSubstring()
        {
            var actual = _target.TrimAll(GetStubs("   Hello    "));
            Assert.AreEqual("Hello", actual.Value);
        }

        [TestMethod]
        public void TrimAll_InsignificantCharacters_ReturnsSubstring()
        {
            var actual = _target.TrimAll(GetStubs("\t \v\r\n\fHello\t\v\r\n\f "));
            Assert.AreEqual("Hello", actual.Value);
        }

        [TestMethod]
        public void TrimAll_WhitespaceLine_ReturnsEmpty()
        {
            var actual = _target.TrimAll(GetStubs("\t\v\r\n\f "));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimAll_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.TrimAll(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimR_Default_ReturnsSubstring()
        {
            var actual = _target.TrimR(GetStubs("   Hello    "));
            Assert.AreEqual("   Hello", actual.Value);
        }

        [TestMethod]
        public void TrimR_InsignificantCharacters_ReturnsSubstring()
        {
            var actual = _target.TrimR(GetStubs("\t \v\r\n\fHello\t\v\r\n\f "));
            Assert.AreEqual("\t \v\r\n\fHello", actual.Value);
        }

        [TestMethod]
        public void TrimR_WhitespaceLine_ReturnsEmpty()
        {
            var actual = _target.TrimR(GetStubs("\t\v\r\n\f "));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimR_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.TrimR(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimL_Default_ReturnsSubstring()
        {
            var actual = _target.TrimL(GetStubs("   Hello    "));
            Assert.AreEqual("Hello    ", actual.Value);
        }

        [TestMethod]
        public void TrimL_InsignificantCharacters_ReturnsSubstring()
        {
            var actual = _target.TrimL(GetStubs("\t \v\r\n\fHello\t\v\r\n\f "));
            Assert.AreEqual("Hello\t\v\r\n\f ", actual.Value);
        }

        [TestMethod]
        public void TrimL_WhitespaceLine_ReturnsEmpty()
        {
            var actual = _target.TrimL(GetStubs("\t\v\r\n\f "));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void TrimL_EmptyLine_ReturnsEmpty()
        {
            var actual = _target.TrimL(GetStubs(string.Empty));
            Assert.AreEqual(string.Empty, actual.Value);
        }

        [TestMethod]
        public void StrLen_Default_ReturnsLength()
        {
            var actual = _target.StrLen(GetStubs("Hello World!!!111"));
            Assert.AreEqual(17.0, actual.Value);
        }

        [TestMethod]
        public void StrLen_InsignificantCharacters_ReturnsFullLength()
        {
            var actual = _target.StrLen(GetStubs("\t\v\r\n\f "));
            Assert.AreEqual(6.0, actual.Value);
        }

        [TestMethod]
        public void StrLen_EmptyString_ReturnsZero()
        {
            var actual = _target.StrLen(GetStubs(string.Empty));
            Assert.AreEqual(0.0, actual.Value);
        }

        #endregion

        #region Functions for working with Number type values

        [TestMethod]
        public void Sqrt_Integer_ReturnsResult()
        {
            var actual = _target.Sqrt(GetStubs(81));
            Assert.AreEqual(9.0, actual.Value);
        }

        [TestMethod]
        public void Sqrt_Double_ReturnsResult()
        {
            var actual = _target.Sqrt(GetStubs(0.81));
            Assert.AreEqual(0.9, actual.Value);
        }

        [TestMethod]
        public void Sqrt_Decimal_ReturnsResult()
        {
            var actual = _target.Sqrt(GetStubs((decimal)0.81));
            Assert.AreEqual(0.9, actual.Value);
        }

        [TestMethod]
        public void Sqrt_Zero_ReturnsZero()
        {
            var actual = _target.Sqrt(GetStubs(0));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void Sqrt_Negative_ReturnsUndefined()
        {
            var actual = _target.Sqrt(GetStubs(-1));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Pow_Default_ReturnsResult()
        {
            var actual = _target.Pow(GetStubs(2, 3));
            Assert.AreEqual(8.0, actual.Value);
        }

        [TestMethod]
        public void Pow_Double_ReturnsResult()
        {
            var actual = _target.Pow(GetStubs(3.5, 5.6));
            Assert.AreEqual(Math.Pow(3.5, 5.6), actual.Value);
        }

        [TestMethod]
        public void Pow_Negative_ReturnsResult()
        {
            var actual = _target.Pow(GetStubs(-2, -3));
            Assert.AreEqual(-0.125, actual.Value);
        }

        [TestMethod]
        public void Pow_ZeroBase_ReturnsZero()
        {
            var actual = _target.Pow(GetStubs(0, 3));
            Assert.AreEqual(0.0, actual.Value);
        }

        [TestMethod]
        public void Pow_ZeroFactor_ReturnsOne()
        {
            var actual = _target.Pow(GetStubs(9, 0));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void Pow_MaxBase_ReturnsUndefined()
        {
            var actual = _target.Pow(GetStubs(double.MaxValue, 2));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Pow_MaxFactor_ReturnsUndefined()
        {
            var actual = _target.Pow(GetStubs(2, double.MaxValue));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Exp_Default_ReturnsResult()
        {
            var actual = _target.Exp(GetStubs(10));
            Assert.AreEqual(Math.Exp(10), actual.Value);
        }

        [TestMethod]
        public void Exp_MaxValue_ReturnsUndefined()
        {
            var actual = _target.Exp(GetStubs(double.MaxValue));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void ATan_Default_ReturnsResult()
        {
            var actual = _target.ATan(GetStubs(1));
            Assert.AreEqual(Math.Atan(1), actual.Value);
        }

        [TestMethod]
        public void ACos_Default_ReturnsResult()
        {
            var actual = _target.ACos(GetStubs(0.5));
            Assert.AreEqual(Math.Acos(0.5), actual.Value);
        }

        [TestMethod]
        public void ACos_MoreThanOne_ReturnsUndefined()
        {
            var actual = _target.ACos(GetStubs(2));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void ASin_Default_ReturnsResult()
        {
            var actual = _target.ASin(GetStubs(0.5));
            Assert.AreEqual(Math.Asin(0.5), actual.Value);
        }

        [TestMethod]
        public void ASin_MoreThanOne_ReturnsUndefined()
        {
            var actual = _target.ASin(GetStubs(2));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Tan_Default_ReturnsResult()
        {
            var actual = _target.Tan(GetStubs(0.5));
            Assert.AreEqual(Math.Tan(0.5), actual.Value);
        }

        [TestMethod]
        public void Cos_Default_ReturnsResult()
        {
            var actual = _target.Cos(GetStubs(0.5));
            Assert.AreEqual(Math.Cos(0.5), actual.Value);
        }

        [TestMethod]
        public void Sin_Default_ReturnsResult()
        {
            var actual = _target.Sin(GetStubs(0.5));
            Assert.AreEqual(Math.Sin(0.5), actual.Value);
        }

        [TestMethod]
        public void Log10_Default_ReturnsResult()
        {
            var actual = _target.Log10(GetStubs(1000));
            Assert.AreEqual(3.0, actual.Value);
        }

        [TestMethod]
        public void Log10_Zero_ReturnsUndefined()
        {
            var actual = _target.Log10(GetStubs(0));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Log10_Negative_ReturnsUndefined()
        {
            var actual = _target.Log10(GetStubs(-100));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Log_Default_ReturnsResult()
        {
            var actual = _target.Log(GetStubs(10));
            Assert.AreEqual(Math.Log(10), actual.Value);
        }

        [TestMethod]
        public void Log_Zero_ReturnsUndefined()
        {
            var actual = _target.Log(GetStubs(0));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Log_Negative_ReturnsUndefined()
        {
            var actual = _target.Log(GetStubs(-100));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Round_Default_ReturnsResult()
        {
            var actual = _target.Round(GetStubs(1.755556, 2));
            Assert.AreEqual(1.76, actual.Value);
        }

        [TestMethod]
        public void Round_NegativeCapacity_ReturnsResult()
        {
            var actual = _target.Round(GetStubs(1464, -2));
            Assert.AreEqual(1500.0, actual.Value);
        }

        [TestMethod]
        public void Round_CapacityMoreThanNumber()
        {
            var actual = _target.Round(GetStubs(12, -2));
            Assert.AreEqual(0.0, actual.Value);
        }

        // TODO: Реализовать тест после добавления параметра
        //[TestMethod]
        //public void Round_RoundingMode0_ReturnsLesserValue()
        //{
        //    var actual = _target.Round(GetStubs( 1.5, 0, 0));
        //    Assert.AreEqual(1, actual.Value);
        //}
        // TODO: Реализовать тест после добавления параметра
        //[TestMethod]
        //public void Round_RoundingMode1_ReturnsHigherValue()
        //{
        //    var actual = _target.Round(GetStubs( 1.5, 0, 1));
        //    Assert.AreEqual(2, actual.Value);
        //}
        // TODO: Реализовать тест после добавления параметра
        //[TestMethod]
        //public void Round_OutOfBoundsRoundingMode_ReturnsUndefined()
        //{
        //    var actual = _target.Round(GetStubs( 3.5555, 0, 42));
        //    Assert.AreEqual(4, actual.Value);
        //}

        [TestMethod]
        public void Round_WithoutCapacity_ReturnInteger()
        {
            var actual = _target.Round(GetStubs(0.55465));
            Assert.AreEqual(1.0, actual.Value);
        }

        [TestMethod]
        public void Int_Default_ReturnLesserInteger()
        {
            var actual = _target.Int(GetStubs(42.99999));
            Assert.AreEqual(42.0, actual.Value);
        }

        [TestMethod]
        public void Int_Negative_ReturnHigherInteger()
        {
            var actual = _target.Int(GetStubs(-42.99999));
            Assert.AreEqual(-42.0, actual.Value);
        }

        #endregion

        #region Functions for working with Date type values

        readonly DateTime STD_DATETIME = new DateTime(1989, 11, 14, 8, 45, 12);
        readonly DateTime STD_DATE = new DateTime(1989, 11, 14);

        [TestMethod]
        public void AddMonth_Positive_ReturnsResult()
        {
            var actual = _target.AddMonth(GetStubs(STD_DATETIME, 12));
            Assert.AreEqual(new DateTime(1990, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void AddMonth_Negative_ReturnsResult()
        {
            var actual = _target.AddMonth(GetStubs(STD_DATETIME, -24));
            Assert.AreEqual(new DateTime(1987, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void WeekDay_Default_ReturnsResult()
        {
            var actual = _target.WeekDay(GetStubs(STD_DATETIME));
            Assert.AreEqual(2.0, actual.ToObject());
        }

        [TestMethod]
        public void DayOfYear_Default_ReturnsResult()
        {
            var actual = _target.DayOfYear(GetStubs(STD_DATETIME));
            Assert.AreEqual(318.0, actual.ToObject());
        }

        [TestMethod]
        public void WeekOfYear_Default_ReturnsResult()
        {
            var actual = _target.WeekOfYear(GetStubs(STD_DATETIME));
            Assert.AreEqual(47.0, actual.ToObject());
        }

        [TestMethod]
        public void EndOfMinute_Default_ReturnsResult()
        {
            var actual = _target.EndOfMinute(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfHour_Default_ReturnsResult()
        {
            var actual = _target.EndOfHour(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfDay_Default_ReturnsResult()
        {
            var actual = _target.EndOfDay(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfWeek_Default_ReturnsResult()
        {
            var actual = _target.EndOfWeek(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 19, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfWeek_AlreadyEndOfWeek_ReturnsResult()
        {
            var actual = _target.EndOfWeek(GetStubs(new DateTime(1989, 11, 19, 8, 45, 59)));
            Assert.AreEqual(new DateTime(1989, 11, 19, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfMonth_Default_ReturnsResult()
        {
            var actual = _target.EndOfMonth(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 30, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfMonth_AlreadyEndOfMonth_ReturnsResult()
        {
            var actual = _target.EndOfMonth(GetStubs(new DateTime(1989, 11, 30, 8, 45, 59)));
            Assert.AreEqual(new DateTime(1989, 11, 30, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfMonth_LeapYear_ReturnsResult()
        {
            var actual = _target.EndOfMonth(GetStubs(new DateTime(2012, 02, 02, 8, 45, 59)));
            Assert.AreEqual(new DateTime(2012, 02, 29, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfQuarter_Default_ReturnsResult()
        {
            var actual = _target.EndOfQuarter(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 12, 31, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfQuarter_AlreadyEndOfQuarter_ReturnsResult()
        {
            var actual = _target.EndOfQuarter(GetStubs(new DateTime(1989, 12, 31, 23, 59, 59)));
            Assert.AreEqual(new DateTime(1989, 12, 31, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void EndOfYear_Default_ReturnsResult()
        {
            var actual = _target.EndOfYear(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 12, 31, 23, 59, 59), actual.ToObject());
        }

        [TestMethod]
        public void BegOfMinute_Default_ReturnsResult()
        {
            var actual = _target.BegOfMinute(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfHour_Default_ReturnsResult()
        {
            var actual = _target.BegOfHour(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfDay_Default_ReturnsResult()
        {
            var actual = _target.BegOfDay(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 14, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfWeek_Default_ReturnsResult()
        {
            var actual = _target.BegOfWeek(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 13, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfWeek_AlreadyBegOfWeek_ReturnsResult()
        {
            var actual = _target.BegOfWeek(GetStubs(new DateTime(1989, 11, 13, 8, 45, 59)));
            Assert.AreEqual(new DateTime(1989, 11, 13, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfMonth_Default_ReturnsResult()
        {
            var actual = _target.BegOfMonth(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 11, 1, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfMonth_AlreadyBegOfMonth_ReturnsResult()
        {
            var actual = _target.BegOfMonth(GetStubs(new DateTime(1989, 11, 1, 8, 45, 59)));
            Assert.AreEqual(new DateTime(1989, 11, 1, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfQuarter_Default_ReturnsResult()
        {
            var actual = _target.BegOfQuarter(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 10, 1, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfQuarter_AlreadyBegOfQuarter_ReturnsResult()
        {
            var actual = _target.BegOfQuarter(GetStubs(new DateTime(1989, 10, 1, 23, 59, 59)));
            Assert.AreEqual(new DateTime(1989, 10, 1, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void BegOfYear_Default_ReturnsResult()
        {
            var actual = _target.BegOfYear(GetStubs(STD_DATETIME));
            Assert.AreEqual(new DateTime(1989, 1, 1, 0, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void Second_Default_ReturnsResult()
        {
            var actual = _target.Second(GetStubs(STD_DATETIME));
            Assert.AreEqual(12.0, actual.ToObject());
        }

        [TestMethod]
        public void Minute_Default_ReturnsResult()
        {
            var actual = _target.Minute(GetStubs(STD_DATETIME));
            Assert.AreEqual(45.0, actual.ToObject());
        }

        [TestMethod]
        public void Hour_Default_ReturnsResult()
        {
            var actual = _target.Hour(GetStubs(STD_DATETIME));
            Assert.AreEqual(8.0, actual.ToObject());
        }

        [TestMethod]
        public void Day_Default_ReturnsResult()
        {
            var actual = _target.Day(GetStubs(STD_DATETIME));
            Assert.AreEqual(14.0, actual.ToObject());
        }

        [TestMethod]
        public void Month_Default_ReturnsResult()
        {
            var actual = _target.Month(GetStubs(STD_DATETIME));
            Assert.AreEqual(11.0, actual.ToObject());
        }

        [TestMethod]
        public void Year_Default_ReturnsResult()
        {
            var actual = _target.Year(GetStubs(STD_DATETIME));
            Assert.AreEqual(1989.0, actual.ToObject());
        }

        #endregion

        #region Value conversion functions

        [TestMethod]
        public void Date_CSharpDefualt_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("14.11.1989 08:45:12"));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void Date_CSharpDefualtOnlyDate_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("14.11.1989"));
            Assert.AreEqual(new DateTime(1989, 11, 14), actual.ToObject());
        }

        [TestMethod]
        public void Date_CSharpInvariant_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("1989/11/14 08:45:12 am"));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void Date_CSharpInvariantOnlyDate_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("1989/11/14"));
            Assert.AreEqual(new DateTime(1989, 11, 14), actual.ToObject());
        }

        [TestMethod]
        public void Date_CSharpNotStandart_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("14 11 1989"));
            Assert.AreEqual(new DateTime(1989, 11, 14), actual.ToObject());
        }

        [TestMethod]
        public void Date_1C_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("19891114084512"));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void Date_1COnlyDate_ReturnsResult()
        {
            var actual = _target.Date(GetStubs("19891114"));
            Assert.AreEqual(new DateTime(1989, 11, 14), actual.ToObject());
        }

        [TestMethod]
        public void Date_ByComponents_ReturnsResult()
        {
            var actual = _target.Date(GetStubs(1989, 11, 14, 8, 45, 12));
            Assert.AreEqual(new DateTime(1989, 11, 14, 8, 45, 12), actual.ToObject());
        }

        [TestMethod]
        public void Date_ByComponentsOnlyDate_ReturnsResult()
        {
            var actual = _target.Date(GetStubs(1989, 11, 14));
            Assert.AreEqual(new DateTime(1989, 11, 14), actual.ToObject());
        }

        [TestMethod]
        public void Date_ByComponentsInvalidInput_ReturnsUndefined()
        {
            var actual = _target.Date(GetStubs(42, 23, 16, 15, 8, 4));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Date_InvalidInput_ReturnsUndefined()
        {
            var actual = _target.Date(GetStubs("4 8 15 16 23 42 "));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void String_Default_ReturnsString()
        {
            MockObject mock = new MockObject();
            var actual = _target.String(GetStubs(mock));
            Assert.AreEqual(MockObject.TO_STRING_MESSAGE, actual.ToObject());
            Assert.IsTrue(mock.ToStringWasExecuted);
        }

        [TestMethod]
        public void String_Null_ReturnsString()
        {
            var actual = _target.String(GetStubs(new object[] { null }));
            Assert.AreEqual(JsNull.Instance.ToString(), actual.ToObject());
        }

        [TestMethod]
        public void Number_InputString_ReturnsInteger()
        {
            var actual = _target.Number(GetStubs("42"));
            Assert.AreEqual(42.0, actual.ToObject());
        }

        [TestMethod]
        public void Number_InputString_ReturnsDouble()
        {
            var actual = _target.Number(GetStubs("42.42"));
            Assert.AreEqual(42.42, actual.ToObject());
        }

        [TestMethod]
        public void Number_InputStringInvariantCulture_ReturnsDouble()
        {
            var actual = _target.Number(GetStubs("42,42"));
            Assert.AreEqual(42.42, actual.ToObject());
        }

        [TestMethod]
        public void Number_InputBool_ReturnsZeroOrOne()
        {
            var actual = _target.Number(GetStubs(true));
            Assert.AreEqual(1.0, actual.ToObject());
        }

        [TestMethod]
        public void Number_InvalidInput_ReturnsUndefined()
        {
            var actual = _target.Number(GetStubs(" error "));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Boolean_InputZero_ReturnsFalse()
        {
            var actual = _target.Boolean(GetStubs(0));
            Assert.AreEqual(false, actual.ToObject());
        }

        [TestMethod]
        public void Boolean_InputHigherZero_ReturnsTrue()
        {
            var actual = _target.Boolean(GetStubs(42));
            Assert.AreEqual(true, actual.ToObject());
        }

        [TestMethod]
        public void Boolean_InputBool_ReturnsBool()
        {
            var actual = _target.Boolean(GetStubs(true));
            Assert.AreEqual(true, actual.ToObject());
        }

        [TestMethod]
        public void Boolean_InputString_ReturnsBool()
        {
            var actual = _target.Boolean(GetStubs("TrUe"));
            Assert.AreEqual(true, actual.ToObject());
        }

        [TestMethod]
        public void Boolean_InvalidInputInt_ReturnsUndefined()
        {
            var actual = _target.Boolean(GetStubs(-1));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Boolean_InvalidInputString_ReturnsUndefined()
        {
            var actual = _target.Boolean(GetStubs(" error "));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        #endregion

        #region Formatting functions

        [TestMethod]
        [Ignore]
        public void Format()
        {

        }
        #endregion

        #region Others

        [TestMethod]
        public void ErrorInfo()
        {
            // TODO: когда будет реализация глобального лога ошибок
        }

        [TestMethod]
        public void Eval()
        {
            // TODO: Eval вызывает метод eval по умолчанию. А его просто так не протестируешь.
        }

        [TestMethod]
        public void ErrorDescription()
        {
            // TODO: когда будет реализация глобального лога ошибок
        }

        [TestMethod]
        public void Max_Number_ReturnsNumber()
        {
            var actual = _target.Max(GetStubs(1, "2", "0,1", 3.5));
            Assert.AreEqual(3.5, actual.ToObject());
        }

        [TestMethod]
        public void Max_String_ReturnsString()
        {
            var actual = _target.Max(GetStubs("AAA", "BBB", "CCC", STD_DATE, 42));
            Assert.AreEqual("CCC", actual.ToObject());
        }

        [TestMethod]
        public void Max_Date_ReturnsDate()
        {
            var actual = _target.Max(GetStubs("2002.07.15 22:00:00", "20020714", STD_DATETIME));
            Assert.AreEqual(new DateTime(2002, 7, 15, 22, 0, 0), actual.ToObject());
        }

        [TestMethod]
        public void Max_Bool_ReturnsBool()
        {
            var actual = _target.Max(GetStubs(true, "fAlse", 0));
            Assert.AreEqual(true, actual.ToObject());
        }

        [TestMethod]
        public void Max_MixedInput_ReturnsString()
        {
            var actual = _target.Max(GetStubs(true, "hello", 42, STD_DATE));
            Assert.AreEqual("True", actual.ToObject());
        }

        [TestMethod]
        public void Max_EmptyInput_ReturnsUndefined()
        {
            var actual = _target.Max(new JsInstance[0]);
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Min_Number_ReturnsNumber()
        {
            var actual = _target.Min(GetStubs(1, "2", "0,1", 3.5));
            Assert.AreEqual(0.1, actual.ToObject());
        }

        [TestMethod]
        public void Min_String_ReturnsString()
        {
            var actual = _target.Min(GetStubs("AAA", "BBB", "CCC", STD_DATE, 42));
            Assert.AreEqual("14.11.1989 0:00:00", actual.ToObject());
        }

        [TestMethod]
        public void Min_Date_ReturnsDate()
        {
            var actual = _target.Min(GetStubs("2002.07.15 22:00:00", "20020714", STD_DATETIME));
            Assert.AreEqual(STD_DATETIME, actual.ToObject());
        }

        [TestMethod]
        public void Min_Bool_ReturnsBool()
        {
            var actual = _target.Min(GetStubs(true, "fAlse", 0));
            Assert.AreEqual(false, actual.ToObject());
        }

        [TestMethod]
        public void Min_MixedInput_ReturnsString()
        {
            var actual = _target.Min(GetStubs(true, "hello", 42, STD_DATE));
            Assert.AreEqual("14.11.1989 0:00:00", actual.ToObject());
        }

        [TestMethod]
        public void Min_EmptyInput_ReturnsUndefined()
        {
            var actual = _target.Min(GetStubs());
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void TypeOf_Default_ReturnResult()
        {
            var actual = _target.TypeOf(GetStubs(""));
            Assert.AreEqual(typeof(string), actual.ToObject());
        }

        [TestMethod]
        public void TypeOf_InputNull_ReturnUndefined()
        {
            var actual = _target.TypeOf(GetStubs(new object[] { null }));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }

        [TestMethod]
        public void Type_FullName_ReturnResult()
        {
            var actual = _target.Type(GetStubs("MobileClient.Tests.ScriptEngine.JsGlobalTests+JsStub"));
            Assert.AreEqual(typeof(JsStub), actual.ToObject());
        }

        [TestMethod]
        public void Type_InvalidInput_ReturnUndefined()
        {
            var actual = _target.Type(GetStubs("error"));
            Assert.IsInstanceOfType(actual, typeof(JsUndefined));
        }
        #endregion

        T Invoke<T>(string name, params object[] args)
        {
            JsInstance[] p = GetStubs(args);

            try
            {
                return (T)_accessor.Invoke(name, new object[] { p });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw;
            }
        }

        static JsInstance[] GetStubs(params object[] args)
        {
            JsInstance[] p = new JsInstance[args.Length];
            for (int i = 0; i < args.Length; i++)
                p[i] = new JsStub(args[i]);
            return p;
        }

        class JsStub : JsInstance
        {
            public JsStub(object value)
            {
                this.Value = value;
            }

            public override bool IsClr
            {
                get { return false; }
            }

            public override object Value { get; set; }

            public override string Class
            {
                get { return "fake"; }
            }
        }

        class MockObject
        {
            public const string TO_STRING_MESSAGE = "success";

            public bool ToStringWasExecuted { get; private set; }

            public override string ToString()
            {
                ToStringWasExecuted = true;
                return TO_STRING_MESSAGE;
            }
        }

        class StubVisitor : IJintVisitor
        {
            public Jint.IPropertyGetter PropertyGetter { get { return null; } }

            public Jint.IMethodInvoker MethodGetter { get { return null; } }

            public Jint.IFieldGetter FieldGetter { get { return null; } }

            public IEntityAccessor EntityAccessor { get { return null; } }

            public JintEngine JintEngine { get; private set; }

            public bool DebugMode
            {
                get { throw new NotImplementedException(); }
            }

            public JsInstance Result
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public IGlobal Global
            {
                get { return null; }
            }

            public JsInstance Returned
            {
                get { throw new NotImplementedException(); }
            }

            public void CallFunction(JsFunction jsFunction, JsDictionaryObject that, JsInstance[] parameters)
            {
                throw new NotImplementedException();
            }

            public JsInstance Return(JsInstance result)
            {
                throw new NotImplementedException();
            }

            public void ExecuteFunction(JsFunction function, JsDictionaryObject _this, JsInstance[] _parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}
