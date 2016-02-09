using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BitMobile.Controls
{
    public class DataBindAttribute : Attribute
    {
        private String fieldName;

        public String FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public DataBindAttribute(String fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}