using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public interface IDbTransaction
    {
        Guid Id { get; }
        void AddObject(object obj);
    }
}
