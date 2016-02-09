using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Common.ScriptEngine;
using Jint.Expressions;
using Jint.Native;

namespace Jint.Debugger
{

    [Serializable]
    public class DebugInformation : EventArgs, IDebugInformation
    {
        public String Module { get; set; }
        public Stack<string> CallStack { get; set; }
        public Statement CurrentStatement { get; set; }
        public JsDictionaryObject Locals { get; set; }
        public JintEngine ScriptEngine { get; set; }

        public int Line
        {
            get { return CurrentStatement.Source.Start.Line; }
        }

        public IDictionary<string, object> LocalValues
        {
            get
            {
                return Locals.ToDictionary(val => val.Key
                    , val => val.Value != null ? val.Value.Value : null);
            }
        }
    }
}
