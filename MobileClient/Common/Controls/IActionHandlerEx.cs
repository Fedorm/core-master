namespace BitMobile.Common.Controls
{
    public interface IActionHandlerEx
    {
        string Expression { get;}
        object Execute();
    }
}
