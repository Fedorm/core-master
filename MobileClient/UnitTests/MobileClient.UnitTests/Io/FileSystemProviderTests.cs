using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.MobileClient.UnitTests.Io
{
    [TestClass]
    public class FileSystemProviderTests
    {
        [ClassInitialize]
        public static void OnClassInitialize(TestContext context)
        {
            D.Init("en");
        }

        [TestMethod]
        public void FilterInvalidCharacters_UsualInput_ReturnsLower()
        {
            string actual = FilterInvalidCharacters(@"ЧмОкЕ/ФсЕм\в/эТаМ/чЯтЕ!!!.eXe");
            Assert.AreEqual(@"чмоке\фсем\в\этам\чяте!!!.exe", actual);
        }

        [TestMethod]
        public void FilterInvalidCharacters_WindowsIllegalCharactes_ReturnsReplacedString()
        {
            string actual = FilterInvalidCharacters(@" root?  /  <>dir \:name*|""^ ");
            Assert.AreEqual(@"root\dir\name", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NonFatalException), "Invalid file name: root/dir/ com1")]
        public void FilterInvalidCharacters_WindowsIllegalName_ThrownException()
        {
            FilterInvalidCharacters(@"root/dir/ com1");
        }

        [TestMethod]
        [ExpectedException(typeof(NonFatalException), "Invalid file name: root/dir/ nul.exe")]
        public void FilterInvalidCharacters_WindowsIllegalNameWithExpansion_ThrownException()
        {
            FilterInvalidCharacters(@"root/dir/ nul.exe");
        }

        [TestMethod]
        [ExpectedException(typeof(NonFatalException), @"Invalid file name: root?/<> \:name*|""^ ")]
        public void FilterInvalidCharacters_BocomesEmpty_ReturnsReplacedString()
        {
            FilterInvalidCharacters(@"root?/<> \:name*|""^ ");
        }

        private string FilterInvalidCharacters(string path)
        {
            return FileSystemProvider.FilterInvalidCharacters(path);
        }
    }
}
