using System;
using BitMobile.DbEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MobileClient.UnitTests.DbEngine
{
    [TestClass]
    public class DbFunctionsTests
    {
        [TestMethod]
        public void Contains_SingleWordValue_ReturnTrue()
        {
            bool actual = DbFunctions.Contains("Hello World!!!", "Hell");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Contains_MultiWordsValue_ReturnTrue()
        {
            bool actual = DbFunctions.Contains("Hello World!!!", "Ell ld");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Contains_MultiWordsValue_ReturnFalse()
        {
            bool actual = DbFunctions.Contains("Hello World!!!", "Ell ld error");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Contains_EmptyInput_ReturnFalse()
        {
            bool actual = DbFunctions.Contains("", "Hello");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Contains_NullInput_ReturnFalse()
        {
            bool actual = DbFunctions.Contains(null, "Hello");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Contains_EmptyValue_ReturnTrue()
        {
            bool actual = DbFunctions.Contains("Hello World!!!", "");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Contains_NullValue_ReturnTrue()
        {
            bool actual = DbFunctions.Contains("Hello World!!!", null);
            Assert.IsTrue(actual);
        }
    }
}
