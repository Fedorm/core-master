using System.Collections.Generic;
using BenchmarkXamarin.Core;
using BitMobile.DataAccessLayer;

namespace BitMobile.Benchmarks
{
    class DalBenchmarks
    {
        private static readonly Dictionary<string, string> Translation = new Dictionary<string, string>()
        {
            {"hello", "Hello"},
            {"price2", "price2"},
            {"stock", "stock"},
            {"brand", "brand"}
        };

        [Benchmark]
        public void TranslateString_Simple()
        {
            string result;
            Dal.TranslateStringInternal(Translation, "#hello#", out result);
        }

        [Benchmark]
        public void TranslateString_Complicated()
        {
            string result;
            Dal.TranslateStringInternal(Translation, "#price2# {$FormatValue($item.Price)}  #stock#: {$item.CommonStock}  #brand#: {$item.Brand}", out result);
        }

        [Benchmark]
        public void TranslateString_WithoutDies()
        {
            string result;
            Dal.TranslateStringInternal(Translation, "$GetQuickOrder($item.Id, $item.Price, pack{$index}, editText{$index}, textView{$index}, $item.RecOrder, $item.UnitId, $item.RecUnit)", out result);
        }
    }
}