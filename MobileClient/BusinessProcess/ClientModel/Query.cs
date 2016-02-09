using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BitMobile.Application.DbEngine;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Develop;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    class Query : ContextAwareObject
    {
        private readonly Dictionary<string, object> _parameters;
        private string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public Query()
        {
            _parameters = new Dictionary<string, object>();
        }

        public Query(string text)
            : this()
        {
            _text = text;
        }

        public void AddParameter(string paramName, object value)
        {
            _parameters.Add(paramName, value);
        }

        public object ExecuteScalar()
        {
            try
            {
                TimeStamp.Start("ExecuteScalar");

                IDatabase db = DbContext.Current.Database;
                var arguments = Prepare();
                return db.SelectScalar(_text, arguments);
            }
            finally
            {
                TimeStamp.Log("ExecuteScalar", _text);
            }
        }
        
        public int ExecuteCount()
        {
            try
            {
                TimeStamp.Start("ExecuteCount");

                int rc = 0;
                using (IDbRecordset rs = Execute())
                    while (rs.Next())
                        rc++;

                return rc;
            }
            finally
            {
                TimeStamp.Log("ExecuteCount", _text);
            }
        }

        public IDbRecordset Execute()
        {
            try
            {
                TimeStamp.Start("Execute");
                return DbContext.Current.CreateDbRecordset(ExecuteInternal());
            }
            finally
            {
                TimeStamp.Log("Execute", _text);
            }
        }

        public void ExecuteInto(string tableName)
        {
            try
            {
                TimeStamp.Start("ExecuteInto");
                ExecuteIntoInternal(tableName);
            }
            finally
            {
                TimeStamp.Log("ExecuteInto", _text);
            }
        }

        private IDataReader ExecuteInternal()
        {
            IDatabase db = DbContext.Current.Database;
            var arguments = Prepare();
            return db.Select(_text, arguments);
        }

        private void ExecuteIntoInternal(String tableName)
        {
            IDatabase db = DbContext.Current.Database;
            var arguments = Prepare();
            db.SelectInto(tableName, _text, arguments);
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private object[] Prepare()
        {
            var arguments = new List<object>();
            foreach (var kvp in _parameters)
            {
                arguments.Add(kvp.Value);
                _text = _text.Replace("@" + kvp.Key, "@p" + arguments.Count);
            }
            return arguments.ToArray<object>();
        }
    }
}