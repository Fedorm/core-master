using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.ValueStack;
using BitMobile.DbEngine;
using BitMobile.Utilities.Develop;

namespace BitMobile.ClientModel
{
    class Query : ContextAwareObject
    {
        private CustomDictionary parameters;
        private String text;

        public String Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public Query()
        {
            parameters = new CustomDictionary();
        }

        public Query(String text)
        {
            parameters = new CustomDictionary();
            this.text = text;
        }

        public void AddParameter(String paramName, object value)
        {
            parameters.Add(paramName, value);
        }

        private object ExecuteInternal()
        {
            BitMobile.DbEngine.Database db = BitMobile.DbEngine.Database.Current;

            List<object> arguments = new List<object>();
            foreach (var kvp in parameters)
            {
                arguments.Add(kvp.Value);
                text = text.Replace("@" + kvp.Key, "@p" + arguments.Count.ToString());
            }

            return (object)db.Select(text, arguments.ToArray<object>());
        }

        private void ExecuteIntoInternal(String tableName)
        {
            BitMobile.DbEngine.Database db = BitMobile.DbEngine.Database.Current;

            List<object> arguments = new List<object>();
            foreach (var kvp in parameters)
            {
                arguments.Add(kvp.Value);
                text = text.Replace("@" + kvp.Key, "@p" + arguments.Count.ToString());
            }

            db.SelectInto(tableName, text, arguments.ToArray<object>());
        }

        public object ExecuteScalar()
        {
            try
            {
                TimeStamp.Start("ExecuteScalar");

                BitMobile.DbEngine.Database db = BitMobile.DbEngine.Database.Current;

                List<object> arguments = new List<object>();
                foreach (var kvp in parameters)
                {
                    arguments.Add(kvp.Value);
                    text = text.Replace("@" + kvp.Key, "@p" + arguments.Count.ToString());
                }

                return db.SelectScalar(text, arguments.ToArray<object>());
            }
            finally
            {
                TimeStamp.Log("ExecuteScalar", text);
            }
        }

        public int ExecuteCount()
        {
            try
            {
                TimeStamp.Start("ExecuteCount");

                int rc = 0;
                using (DbRecordset rs = Execute())
                {
                    while (rs.Next())
                    {
                        rc++;
                    }
                }
                return rc;
            }
            finally
            {
                TimeStamp.Log("ExecuteCount", text);
            }
        }

        public DbRecordset Execute()
        {
            try
            {
                TimeStamp.Start("Execute");
                return new DbRecordset(ExecuteInternal());
            }
            finally
            {
                TimeStamp.Log("Execute", text);
            }
        }

        public void ExecuteInto(String tableName)
        {
            try
            {
                TimeStamp.Start("ExecuteInto");
                ExecuteIntoInternal(tableName);
            }
            finally
            {
                TimeStamp.Log("ExecuteInto", text);
            }
        }

    }
}