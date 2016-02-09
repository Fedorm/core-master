using System;

namespace BitMobile.Common.DbEngine
{
    public interface IDbTransaction
    {
        Guid Id { get; }
        void AddObject(object obj);
    }
}
