using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace BitMobile.MobileClient.UnitTests.Dal
{
    [TestClass]
    public class DataAccessLayerTests
    {
        private static readonly Dictionary<string, string> Translation = new Dictionary<string, string>
        {
            {"hello", "TRANSLATION"},
            {"price2", "TRANSLATION"},
            {"stock", "TRANSLATION"},
            {"brand", "TRANSLATION"}
        };

        [TestMethod]
        public void TranslateString_Simple()
        {
            string actual;
            bool changed = DataAccessLayer.Dal.TranslateStringInternal(Translation, "  #hello#  ", out actual);
            Assert.AreEqual("  TRANSLATION  ", actual);
            Assert.AreEqual(true, changed);
        }

        [TestMethod]
        public void TranslateString_Complicated()
        {
            string actual;
            bool changed = DataAccessLayer.Dal.TranslateStringInternal(Translation
                , "#price2# {$FormatValue($item.Price)}  #stock#: {$item.CommonStock}  #brand#: {$item.Brand} #not#"
                , out actual);

            Assert.AreEqual(
                "TRANSLATION {$FormatValue($item.Price)}  TRANSLATION: {$item.CommonStock}  TRANSLATION: {$item.Brand} #not#", actual);
            Assert.AreEqual(true, changed);
        }

        [TestMethod]
        public void TranslateString_Empty()
        {
            string actual;
            bool changed = DataAccessLayer.Dal.TranslateStringInternal(Translation
                , "$CreateOrderItem(editText{$index}, textView{$index}, pack{$index}, $item.Id, $item.Price, swipe_layout{$index}, $item.RecOrder, $item.UnitId)"
                , out actual);
            
            Assert.AreEqual("$CreateOrderItem(editText{$index}, textView{$index}, pack{$index}, $item.Id, $item.Price, swipe_layout{$index}, $item.RecOrder, $item.UnitId)", actual);
            Assert.AreEqual(false, changed);
        }
    }
}
