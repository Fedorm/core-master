using System;
using System.Collections.Generic;
using System.Threading;

using Jint;

namespace BitMobile.Script
{
    public class CachedTypeResolver : ITypeResolver
    {
        static Dictionary<string, Type> _Cache = new Dictionary<string, Type>();
        static ReaderWriterLock rwl = new ReaderWriterLock();

        static CachedTypeResolver()
        {
            //_Cache.Add("Query", typeof(Query));
        }

		public static void RegisterType(String name, Type type)
		{
			_Cache.Add(name, type);
		}

        public Type ResolveType(string fullname)
        {
            rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (_Cache.ContainsKey(fullname))
                {
                    return _Cache[fullname];
                }
                else
                    throw new Exception(String.Format("Cant resolve type '{0}'", fullname));
            }
            finally
            {
                rwl.ReleaseReaderLock();
            }
       }
    }
}
