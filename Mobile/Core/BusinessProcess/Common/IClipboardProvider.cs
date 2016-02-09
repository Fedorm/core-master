namespace BitMobile.Common
{
    public interface IClipboardProvider
    {
        bool HasStringValue { get; }
        bool SetString(string str);
        string GetString();
    }
}