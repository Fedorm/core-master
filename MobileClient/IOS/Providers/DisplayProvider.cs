using BitMobile.Common.Device.Providers;
using JMABarcodeMT;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class DisplayProvider : IDisplayProvider
    {
        #region IDisplayProvider implementation

        public float Width
        {
            get { return UIScreen.Screens[0].ApplicationFrame.Width; }
        }

        public float Height
        {
            get { return UIScreen.Screens[0].ApplicationFrame.Height; }
        }

        public double PxPerMm
        {
            get
            {
                float pixelPerMm = DeviceHardware.Dpi/25.4f;
                return pixelPerMm/UIScreen.MainScreen.Scale;
            }
        }

        #endregion
    }
}