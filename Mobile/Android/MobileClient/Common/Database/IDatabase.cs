using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace BitMobile.DbEngine
{
    public interface IDatabase
    {
        void OnStart();
        System.Data.DataTable SelectAsDataTable(String name, String query, params object[] arguments);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
