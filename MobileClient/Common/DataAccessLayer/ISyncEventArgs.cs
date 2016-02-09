using System;

namespace BitMobile.Common.DataAccessLayer
{
    public interface ISyncEventArgs
    {
        Exception Exception { get; }
        bool Ok { get; }
    }
}
