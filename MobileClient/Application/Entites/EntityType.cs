using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BitMobile.Common.Entites;

namespace BitMobile.Application.Entites
{
    [DebuggerDisplay("{_tableName}")]
    public struct EntityType : IEntityType
    {
        private readonly Dictionary<string, IEntityField> _fields;
        private readonly string _schema;
        private readonly string _name;
        private readonly string _tableName;

        internal EntityType(string schema, string name, IEnumerable<IEntityField> fields, string idName)
            : this()
        {
            _schema = schema;
            _name = name;
            _tableName = string.Format("{0}_{1}", _schema, _name);
            _fields = fields.ToDictionary(val => val.Name);
            IdFieldName = idName;
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

        public string TypeName
        {
            get { return string.Format("DefaultScope.{0}.{1}", _schema, _name); }
        }

        public ICollection<IEntityField> Fields
        {
            get { return _fields.Values; }
        }

        public string IdFieldName { get; private set; }

        public IEntityField GetField(string name)
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

        public override int GetHashCode()
        {
            return _tableName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is EntityType)
            {
                var type = (EntityType)obj;
                if (type._tableName == _tableName)
                {
                    if (type._fields.Count != _fields.Count)
                        throw new Exception("Invalid Equals of EntityType");
                    return true;
                }
            }
            return false;
        }
    }
}