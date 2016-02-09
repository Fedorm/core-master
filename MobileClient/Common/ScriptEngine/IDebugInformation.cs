using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Common.ScriptEngine
{
    public interface IDebugInformation
    {
        string Module { get; }
        int Line { get; }
        IDictionary<string, object> LocalValues { get; }
    }
}
