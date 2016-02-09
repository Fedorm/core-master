namespace BitMobile.Common.ValueStack
{
    public interface IIteratorStatus
    {
        int Index { get; set; }
        void Inc();
    }
}
