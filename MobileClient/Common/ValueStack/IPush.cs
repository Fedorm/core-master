namespace BitMobile.Common.ValueStack
{
    public interface IPush
    {
        string Id { get; set; }
        string Type { get; set; }
        string Value { get; set; }
    }
}
