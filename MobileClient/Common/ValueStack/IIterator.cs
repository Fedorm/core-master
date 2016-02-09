namespace BitMobile.Common.ValueStack
{
    public interface IIterator
    {
        string Id { get; set; }
        string Value { get; set; }
        string Status { get; set; }
    }
}
