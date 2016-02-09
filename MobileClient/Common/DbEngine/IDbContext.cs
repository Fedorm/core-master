using System.Data;
using System.IO;

namespace BitMobile.Common.DbEngine
{
    public interface IDbContext: IDbRefFactory
    {
        IDatabase Database { get; }
        IDbRecordset CreateDbRecordset(IDataReader reader);
        void CreateDatabase();
        void InitDatabase(string solutionUri);
        void ReplaceDatabase(byte[] db);
        Stream GetDatabaseStream();
    }
}
