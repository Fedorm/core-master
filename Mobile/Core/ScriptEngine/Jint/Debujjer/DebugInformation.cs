using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Jint.Native;

using BitMobile.ValueStack;

namespace Jint.Debugger
{
    [Serializable]
    public class DebugInformation : EventArgs
    {
        public String Module { get; set; }
        public Stack<string> CallStack { get; set; }
        public Statement CurrentStatement { get; set; }
        public JsDictionaryObject Locals { get; set; }
        public JintEngine ScriptEngine { get; set; }
    }
}
