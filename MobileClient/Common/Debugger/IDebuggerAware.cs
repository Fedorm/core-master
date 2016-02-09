namespace BitMobile.Common.Debugger
{
    interface IDebuggerAware
    {
        void InjectDebugger(IDebugger debugger);
    }
}
