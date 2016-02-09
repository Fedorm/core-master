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

        private String status;

        public String Status
        {
            get { return status; }
            set { status = value; }
        }

        public Iterator()
        {
        }
    }

    public class IteratorStatus
    {
        private int index;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public IteratorStatus()
        {
            index = 0;
        }

        public void Inc()
        {
            index++;
        }
    }
}