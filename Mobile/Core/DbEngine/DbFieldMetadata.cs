using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public class DbFieldMetadata
    {
        private String tableName;
        public String TableName
        {
            get
            {
                return tableName;
            }
        }

        public DbFieldMetadata(String tableName)
        {
            this.tableName = tableName;
        }
    }
}
