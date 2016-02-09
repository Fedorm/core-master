using System;
using BitMobile.Common.Log;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;

namespace BitMobile.Common.DbEngine
{
    public interface IDbRef: ILoggable
    {
        Guid Id { get; }
        String TableName { get; }
        bool HasCache { get; }
        IEntity GetObject();
        bool EmptyRef();
        bool IsNew();
    }
}
