namespace BitMobile.Common.Controls
{
    public interface ILayoutableContainer : IContainer, ILayoutable
    {
        void Insert(int index, object obj);
        void Withdraw(int index);
        void Inject(int index, string xml);
        void CreateChildrens();
    }
}