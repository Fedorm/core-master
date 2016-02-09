using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public class DbTransaction : IDbTransaction
    {
        private Guid id;
        private Database db;
        private List<object> refs;

        public Guid Id
        {
            get
            {
                return id;
            }
        }

        public void AddObject(object obj)
        {
            refs.Add(obj);
        }

        public DbTransaction(Database db)
        {
            this.db = db;
            this.id = Guid.NewGuid();
            this.refs = new List<object>();
        }
    }
}
