using System;

namespace BitMobile.DbEngine
{
    public static class DbHelper
    {
        public static object DbValue(this object v)
        {
            if (v != null)
            {
                var s = v as string;
                if (s != null && DbRef.CheckIsRef(s))
                    return DbRef.FromString(s);

                if (v == DBNull.Value)
                    return null;
            }
            return v;
        }
    }
}
