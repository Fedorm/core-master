using BitMobile.Common.Debugger;

namespace BitMobile.Debugger
{
    public class DebugContext: IDebugContext
    {
        public IDebugger CreateDebugger()
        {
            return Debugger.CreateInstance();
        }
    }
}
