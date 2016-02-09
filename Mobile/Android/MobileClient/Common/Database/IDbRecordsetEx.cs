using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public interface IDbRecordsetEx : IDbRecordset
    {
        void First();
        int Count();
    }
}
