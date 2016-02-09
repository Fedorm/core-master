using System;

namespace BenchmarkXamarin.Core
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BenchmarkAttribute : Attribute
    {
    }
}

