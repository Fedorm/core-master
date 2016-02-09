using System;
using System.Collections.Generic;
using System.Text;


namespace BitMobile.ValueStack
{
    public class Push : ValueStackTag
    {
        private String id;

        public String Id
        {
            get { return id; }
            set { id = value; }
        }

        private String type;

        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        private String value;

        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private String singleOrDefault;

        public String SingleOrDefault
        {
            get { return this.singleOrDefault; }
            set { this.singleOrDefault = value; }
        }

        private String where;

        public String Where
        {
            get { return where; }
            set { where = value; }
        }

        private String orderBy;

        public String OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        public Push()
        {
        }
    }
}