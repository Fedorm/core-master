using System;

namespace BitMobile.DbEngine
{
    public interface ISqliteEntity
    {
        Guid EntityId { get; }
        bool IsTombstone { get; set; }
        //bool IsNew { get; set; }
        bool IsNew();
        bool IsModified();
        void Load(bool isNew);
        void Save();
        void Save(bool inTran);
    }
}