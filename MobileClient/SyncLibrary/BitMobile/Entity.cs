using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Application.DbEngine;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.ValueStack;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;

namespace BitMobile.SyncLibrary.BitMobile
{
    public class Entity : IsolatedStorageOfflineEntity
    {
        private readonly object[] _values;

        public Entity(IEntityType type)
        {
            EntityType = type;
            _values = new object[EntityType.Fields.Count];
        }

        public static IEntity CreateInstance(IEntityType type)
        {
            var entity = new Entity(type);
            entity.SetIsNew();
            foreach (IEntityField field in type.Fields)
            {
                object value = field.Name == "Id"
                    ? DbContext.Current.CreateDbRef(type.TableName, Guid.NewGuid())
                    : GetDefaultValue(field);

                entity.SetValue(field.Name, value);
            }

            return entity;
        }

        public void SetDbRefValue(string name, string tableName, string value)
        {
            var dbRef = DbContext.Current.CreateDbRef(tableName, Guid.Parse(value));
            SetValue(name, dbRef);
        }

        #region IEntity

        public override void SetValue(string propertyName, object value)
        {
            IEntityField field = EntityType.GetField(propertyName);
            _values[field.Index] = Parse(field.Type, value);
            OnPropertyChanged();
        }

        public override string GetString()
        {
            var builder = new StringBuilder();
            builder.Append("|");
            foreach (var value in _values)
            {
                string str = value != null ? value.ToString() : "null";
                builder.Append(str);
                builder.Append("|");
            }
            return builder.ToString();
        }

        public override object GetValue(string propertyName)
        {
            IEntityField field = EntityType.GetField(propertyName);
            return _values[field.Index] ?? GetDefaultValue(field);
        }

        public override bool HasProperty(string propertyName)
        {
            return EntityType.Exists(propertyName);
        }
        #endregion

        private static object GetDefaultValue(IEntityField field)
        {
            if (field.Type == typeof(IDbRef))
                return DbContext.Current.CreateDbRef(field.DbRefTable, Guid.Empty);

            if (field.AllowNull)
                return null;

            if (field.Type.IsValueType)
                return Activator.CreateInstance(field.Type);

            if (field.Type == typeof(string))
                return string.Empty;

            throw new Exception("Unknown type: " + field.Type);
        }
    }
}