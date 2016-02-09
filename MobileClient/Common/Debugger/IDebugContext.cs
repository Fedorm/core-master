namespace BitMobile.Common.Debugger
{
    public interface IDebugContext
    {
        IDebugger CreateDebugger();
    }
}