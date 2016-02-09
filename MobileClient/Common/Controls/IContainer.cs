namespace BitMobile.Common.Controls
{
    public interface IContainer
    {
        object[] Controls { get; }
        void AddChild(object obj);
        object GetControl(int index);
    }
}