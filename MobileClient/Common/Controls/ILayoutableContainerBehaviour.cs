using System.Collections.Generic;

namespace BitMobile.Common.Controls
{
    public interface ILayoutableContainerBehaviour<T>
    {
        IList<T> Childrens { get; }
        void Insert(int index, object obj);
        void Insert(int index, T child);
        void Withdraw(int index);
        void Inject(int index, string xml);
    }
}