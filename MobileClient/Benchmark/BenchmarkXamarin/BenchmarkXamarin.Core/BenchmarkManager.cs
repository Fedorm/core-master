using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BenchmarkXamarin.Core
{
    public class BenchmarkManager
    {
        private const int WarmupCount = 100;
        private const int WorkingCount = 1000;
        private static readonly object[] Args = new object[0];
        private IEnumerable<MethodInfo> _benchmarks;
        
        public event LogEventHandler Log = Console.WriteLine;
        
        public void Start()
        {
            if (_benchmarks == null)
            {
                Log("looking for benchmarks");
                _benchmarks = FindBenchmarks();
                Log("benchmarks loaded");
            }

            foreach (MethodInfo benchmark in _benchmarks)
                Perform(benchmark);
        }

        private IEnumerable<MethodInfo> FindBenchmarks()
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   where assembly.GetCustomAttribute<BenchmarkAttribute>() != null
                   from type in assembly.GetTypes()
                   from mi in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   where mi.GetCustomAttribute<BenchmarkAttribute>() != null
                   select mi;
        }

        private void Perform(MethodInfo benchmark)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("------------------------ start {0} ---------------------", benchmark.Name);

            object obj = null;
            if (benchmark.DeclaringType != null)
                obj = Activator.CreateInstance(benchmark.DeclaringType);

            // warmup
            for (int i = 0; i < WarmupCount; i++)
                Execute(benchmark, obj);
            
            // work
            var stopwatch = new Stopwatch();
            for (int i = 0; i < WorkingCount; i++)
                Execute(benchmark, obj, stopwatch);

            double elapsed = Convert.ToDouble(stopwatch.ElapsedMilliseconds);
            double average = elapsed / WorkingCount;

            Log(string.Format("{0}: {1}ms", benchmark.Name, average));
        }

        private static void Execute(MethodInfo benchmark, object obj, Stopwatch stopwatch = null)
        {
            if (stopwatch != null)
                stopwatch.Start();

            benchmark.Invoke(obj, Args);

            if (stopwatch != null)
                stopwatch.Stop();
        }
    }
}
