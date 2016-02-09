using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public enum ProcessMode
    {
        InitialLoad = 1,
        ServerChanges = 2,
        LocalChanges = 3
    }
}
