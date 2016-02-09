using System;
using BitMobile.Common.DbEngine;

namespace BitMobile.Common.ValueStack
{
    public interface ICommonData
    {
        DateTime Now { get; }
        Guid UserId { get; set; }
        IDbRef UserRef { get; }
        string LastUpdated { get; }
        DateTime Today { get; }
        string LoadingStatus { get; set; }
        string OS { get; }
        bool SyncIsOK { get; set; }
    }
}
