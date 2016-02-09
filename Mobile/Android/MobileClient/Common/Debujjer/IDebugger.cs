using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Debugger
{
    public interface IDebugger
    {
        EventHandler<EventArgs> OnBreak { get; }

        int[] GetBreakPoints(String moduleName);

        void WriteToConsole(String message);
    }
}
