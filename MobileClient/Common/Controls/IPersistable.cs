namespace BitMobile.Common.Controls
{
    public interface IPersistable
    {
        object GetState();
        void SetState(object state);
    }
}