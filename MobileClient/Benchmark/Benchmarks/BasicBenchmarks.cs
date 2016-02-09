using BenchmarkXamarin.Core;

namespace BitMobile.Benchmarks
{
    class BasicBenchmarks
    {
        [Benchmark]
        public void Empty()
        {
        }

        [Benchmark]
        public void Assignment()
        {
            int i = 10;
            string s = "Hello World";
            var v = new BasicBenchmarks();
        }

        [Benchmark]
        public void MethodInvocation()
        {
            TestMethod(10);
        }

        private void TestMethod(int i)
        {
            if (i > 0)
                TestMethod(i - 1);
        }

    }
}