using BitMobile.Common.Device.Providers;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class ClipboardProvider : IClipboardProvider
    {
        #region IClipboardProvider implementation

        public bool SetString(string str)
        {
            if (str == null)
                return false;

            UIPasteboard.General.String = str;
            return true;
        }

        public string GetString()
        {
            return UIPasteboard.General.String;
        }

        public bool HasStringValue
        {
            get { return !string.IsNullOrEmpty(UIPasteboard.General.String); }
        }

        #endregion
    }
}