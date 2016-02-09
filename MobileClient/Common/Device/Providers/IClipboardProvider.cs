namespace BitMobile.Common.Device.Providers
{
    public interface IClipboardProvider
    {
        bool HasStringValue { get; }
        bool SetString(string str);
        string GetString();
    }
}