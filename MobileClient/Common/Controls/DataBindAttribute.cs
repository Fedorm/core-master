using System;

namespace BitMobile.Controls
{
    public class DataBindAttribute : Attribute
    {
        public string FieldName { get; private set; }

        public DataBindAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}