using BitMobile.Common.Controls;

namespace BitMobile.Controls
{
    class ActionHandlerLocker: ILocker
    {
        public bool Locked { get; private set; }

        public void Acquire()
        {
            Locked = true;
        }

        public void Release()
        {
            Locked = false;
        }
    }
}
