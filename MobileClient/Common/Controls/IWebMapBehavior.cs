namespace BitMobile.Common.Controls
{
    public interface IWebMapBehavior
    {
        string Page { get; }
        void AddMarker(string caption, double latitude, double longitude, string color);
        string BuildShowMarkerFunction(string caption, double latitude, double longitude, string color);
        string BuildInitializeFunction();
    }
}
