namespace BitMobile.Common.Controls
{
    public interface IControl<out T> : ILayoutable
    {
        T View { get; }
    }
}
