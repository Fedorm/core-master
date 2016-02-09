using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Debugger
{
    interface IDebuggerAware
    {
        void InjectDebugger(IDebugger debugger);
    }
}
