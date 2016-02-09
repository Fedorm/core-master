namespace BitMobile.Common.Controls
{
    public interface IActionHandler
    {
        string Expression { get; }
        object Execute();
    }
}
