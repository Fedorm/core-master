using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using BitMobile.Common.IO;

namespace BitMobile.Application.IO
{
    // ReSharper disable once InconsistentNaming
    public static class IOContext
    {
        public static IIOContext Current { get; private set; }

        public static void Init(IIOContext context)
        {
            Current = context;
        }
    }
}
