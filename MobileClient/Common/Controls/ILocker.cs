namespace BitMobile.Common.Controls
{
    public interface ILocker
    {
        bool Locked { get; }
        void Acquire();
        void Release();
    }
}
