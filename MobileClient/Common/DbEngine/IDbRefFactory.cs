using System;

namespace BitMobile.Common.DbEngine
{
    public interface IDbRefFactory
    {
        IDbRef CreateDbRef(string tableName, Guid id);
        IDbRef CreateDbRef(string str);
    }
}