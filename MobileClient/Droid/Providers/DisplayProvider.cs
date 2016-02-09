using BitMobile.Common.Device.Providers;

namespace BitMobile.Droid.Providers
{
    class DisplayProvider : IDisplayProvider
    {
        public float Width
        {
            get { return BitBrowserApp.Current.Width; }
        }

        public float Height
        {
            get { return BitBrowserApp.Current.Height; }
        }

        public double PxPerMm
        {
            get
            {
                var metrics = BitBrowserApp.Current.Resources.DisplayMetrics;
                return ((int)metrics.DensityDpi) / 25.4;
            }
        }
    }
}
