using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using BitMobile.Common.Entites;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;

namespace BitMobile.Common.DbEngine
{
    public interface IDatabase
    {
        DateTime LastSyncTime { get; }
        string UserId { get; }
        string UserEmail { get; }
        string ResourceVersion { get; }
        bool SuccessSync { get; }
        string LocationsTable { get; }
        string LogTable { get; }
        void OnStart();
        bool IsSynced();
        bool InTransaction();
        DataTable SelectAsDataTable(String name, String query, params object[] arguments);
        void CommitTransaction();
        void RollbackTransaction();
        IEntity SelectById(IEntityType type, String guid);
        IDataReader Select(String query, params object[] arguments);
        object SelectScalar(string query, params object[] arguments);
        Stream SelectStream(string query, params object[] arguments);
        IEnumerable SelectDirty(IEntityType type);
        int ExecuteNonQuery(string query, params object[] arguments);
        void Delete(IDbRef obj, bool inTran);
        void SyncComplete(bool success);
        byte[] LoadAnchor();
        void SaveAnchor(byte[] anchor);
        void ProcessData(IEnumerable<IEntity> data, ProcessMode mode);
        void CreateEntityTables(IEnumerable<IEntityType> types);
        void InitialLoadComplete(string userId, string userEmail, string resourceVersion);
        void UpdateConnectionInfo(string userId, string userEmail, string resourceVersion);
        void Save(IEntity obj, bool inTran);
        void SelectInto(string tableName, string query, params object[] arguments);
        void CreateUserTable(string tableName, params string[] columns);
        void DropUserTable(string tableName);
        void TruncateUserTable(string tableName);
    }
}
