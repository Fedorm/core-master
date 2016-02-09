namespace BitMobile.Common.ValueStack
{
    public interface IIf
    {
        string Test { get; set; }
        bool Evaluate(object value);
    }
}
