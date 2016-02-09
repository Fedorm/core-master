using System;

namespace BitMobile.Common.Debugger
{
    public interface IDebugger
    {
        EventHandler<EventArgs> OnBreak { get; }

        int[] GetBreakPoints(String moduleName);

        void WriteToConsole(String message);
    }
}
