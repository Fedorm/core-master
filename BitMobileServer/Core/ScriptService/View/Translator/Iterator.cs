using System;
using System.Collections.Generic;
using System.Text;


namespace BitMobile.ValueStack
{
    public class Iterator : ValueStackTag
    {
        private String id;

        public String Id
        {
            get { return id; }
            set { id = value; }
        }
        private String value;

        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public Iterator()
        {
        }
    }
}