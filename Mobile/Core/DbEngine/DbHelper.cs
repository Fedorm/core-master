using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public static class DbHelper
    {
        public static object DbValue(this object v)
        {
            if (v != null)
            {
                if (DbEngine.DbRef.CheckIsRef(v.ToString()))
                    return DbEngine.DbRef.FromString(v.ToString());

                if (v == DBNull.Value)
                    return null;
            }
            return v;
        }
    }
}
