using System;
using System.Data;
using System.IO;
using BitMobile.Application.IO;
using BitMobile.Common.DbEngine;

namespace BitMobile.DbEngine
{
    public class DbContext : IDbContext
    {
        public IDatabase Database
        {
            get { return DbEngine.Database.Current; }
        }
        
        public IDbRef CreateDbRef(string tableName, Guid id)
        {
            return DbRef.CreateInstance(tableName, id);
        }

        public IDbRef CreateDbRef(string str)
        {
            if (DbRef.CheckIsRef(str))
                return DbRef.FromString(str);
            throw new Exception("Cannot parse Dbref " + str);
        }

        public IDbRecordset CreateDbRecordset(IDataReader reader)
        {
            return new DbRecordset(reader);
        }

        public void CreateDatabase()
        {
            DbEngine.Database.CreateDatabase();
        }

        public void InitDatabase(string solutionUri)
        {
            DbEngine.Database.Init(solutionUri);
        }

        public void ReplaceDatabase(byte[] db)
        {            
            if (DbEngine.Database.Current != null)
            {
                DbEngine.Database.Current.RestoreBackup(db);
            }
        }

        public Stream GetDatabaseStream()
        {
            var copy = DbEngine.Database.Current.GetBackup();
            return  new MemoryStream(copy);
            //string path = DbEngine.Database.GetDbPath();            
            //return IOContext.Current.FileStream(path, FileMode.Open);
        }
    }
}