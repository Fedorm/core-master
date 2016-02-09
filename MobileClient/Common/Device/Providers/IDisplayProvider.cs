namespace BitMobile.Common.Device.Providers
{
    public interface IDisplayProvider
    {
        float Width { get; }
        float Height { get; }
        double PxPerMm { get; }
    }
}
