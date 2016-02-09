using System;
using BitMobile.ValueStack;

namespace BitMobile.DbEngine
{
    public interface IDbRef
    {
        Guid Id { get; }
        String TableName { get; }
        IEntity GetObject();
        bool EmptyRef();
        bool IsNew();
    }
}
