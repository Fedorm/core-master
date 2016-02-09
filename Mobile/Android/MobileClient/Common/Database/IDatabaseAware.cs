using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public interface IDatabaseAware
    {
        void SetDatabase(BitMobile.DbEngine.IDatabase database);
    }
}
