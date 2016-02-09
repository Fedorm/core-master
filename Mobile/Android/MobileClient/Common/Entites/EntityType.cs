using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using BitMobile.DbEngine;
using BitMobile.ValueStack;

namespace BitMobile.Common.Entites
{
    [DebuggerDisplay("{_tableName}")]
    public struct EntityType
    {
        private readonly Dictionary<string, EntityField> _fields; 
        private readonly string _schema;
        private readonly string _name;
        private readonly string _tableName;

        internal EntityType(string schema, string name, IEnumerable<EntityField> fields)
        {
            _schema = schema;
            _name = name;
            _tableName = String.Format("{0}_{1}", _schema, _name);
            _fields = fields.ToDictionary(val => val.Name);
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public ICollection<EntityField> Fields
        {
            get { return _fields.Values; }
        }

        public EntityField GetField(string name)
        {
            return _fields[name];
        }

        public int GetPropertyIndex(string name)
        {
            return _fields[name].Index;
        }

        /// <summary>
        /// Не верно!!!
        /// </summary>
        public bool IsTable
        {
            get { return true; }
        }

        public string[] GetProperties()
        {
            return _fields.Keys.ToArray();
        }

        public string[] GetColumns()
        {
            return GetProperties();
        }

        public bool Exists(string propertyName)
        {
            return _fields.ContainsKey(propertyName);
        }

        public Type GetPropertyType(string propertyName)
        {
            return _fields[propertyName].Type;
        }

        public bool IsPrimaryKey(string propertyName)
        {
            return _fields[propertyName].KeyField;
        }

        public bool IsIndexed(string propertyName)
        {
            return false;
        }
    }
}