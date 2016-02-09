using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.DbEngine
{
    public class TableAttribute : Attribute
    {
    }

    public class ColumnAttribute : Attribute
    {
    }

    public class PrimaryKeyAttribute : Attribute
    {
    }

    public class IndexedAttribute : Attribute
    {
    }

    public class LinkedColumnAttribute : ColumnAttribute
    {
        private String linkedTable;
        public String LinkedTable
        {
            get
            {
                return linkedTable;
            }
        }

        public LinkedColumnAttribute(String linkedTable)
            : base()
        {
            this.linkedTable = linkedTable;
        }
    }
}
