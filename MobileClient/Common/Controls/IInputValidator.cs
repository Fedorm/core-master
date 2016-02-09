namespace BitMobile.Common.Controls
{
    public interface IInputValidator
    {
        bool IsNumeric { get; set; }
        void OnChange(string input, string old);
    }
}