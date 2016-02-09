using System;
using System.Data;
using System.Globalization;
using System.Linq;
using BitMobile.Common.Entites;
using BitMobile.DbEngine;
using BitMobile.ValueStack;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;

namespace BitMobile.SyncLibrary.BitMobile
{
    public class Entity : IsolatedStorageOfflineEntity
    {
        private readonly object[] _values;

        public Entity(EntityType type)
        {
            EntityType = type;
            _values = new object[EntityType.Fields.Count];
        }

        public static IEntity CreateInstance(EntityType type)
        {
            var entity = new Entity(type);
            entity.SetIsNew();
            foreach (EntityField field in type.Fields)
            {
                object value = field.Name == "Id" 
                    ? DbRef.CreateInstance(type.TableName, Guid.NewGuid()) 
                    : GetDefaultValue(field);

                entity.SetValue(field.Name, value);
            }
            return entity;
        }

        public void SetDbRefValue(string name, string tableName, string value)
        {
            var dbRef = DbRef.CreateInstance(tableName, Guid.Parse(value));
            SetValue(name, dbRef);
        }

        #region IEntity

        public override void SetValue(string propertyName, object value)
        {
            EntityField field = EntityType.GetField(propertyName);
            _values[field.Index] = Parse(field.Type, value);
            OnPropertyChanged();
        }

        public override object GetValue(string propertyName)
        {
            EntityField field = EntityType.GetField(propertyName);
            return _values[field.Index] ?? GetDefaultValue(field);
        }

        public override bool HasProperty(string propertyName)
        {
            return EntityType.Exists(propertyName);
        }
        #endregion


        private static object GetDefaultValue(EntityField field)
        {
            if (field.Type == typeof(IDbRef))
                return DbRef.CreateInstance(field.DbRefTable, Guid.Empty);

            if (field.AllowNull)
                return null;

            if (field.Type.IsValueType)
                return Activator.CreateInstance(field.Type);

            if (field.Type == typeof(string))
                return string.Empty;

            throw new Exception("Unknown type: " + field.Type);
        }

        private object Parse(Type type, object value)
        {
            object result = null;
            bool isNullable = !type.IsValueType;
            Type t = Nullable.GetUnderlyingType(type);
            if (t == null)
            {
                t = type;
                isNullable = true;
            }

            if (value == null)
                return isNullable ? null : Activator.CreateInstance(type);

            var str = value as string;
            if (str != null)
            {
                if (t == typeof(IDbRef))
                {
                    if (DbRef.CheckIsRef(str))
                        result = DbRef.FromString(str);
                    else
                        throw new Exception("Invalid DBRef: " + str);
                }
                else if (type == typeof(string))
                    result = str;
                else if (t == typeof(Guid))
                    result = Guid.Parse(str);
                else if (t == typeof(DateTime))
                    result = DateTime.Parse(str);
                else if (t == typeof(Int32))
                    result = Int32.Parse(str);
                else if (t == typeof(Decimal))
                {
                    decimal dec;
                    if (!Decimal.TryParse(str, out dec))
                        dec = Decimal.Parse(str, CultureInfo.InvariantCulture);
                    result = dec;
                }
                else if (t == typeof(Boolean))
                    result = Boolean.Parse(str);
            }
            else if (t.IsInterface && value != null && value.GetType().GetInterfaces().Contains(t))
            {
                result = value;
            }
            else
            {
                try
                {
                    result = Convert.ChangeType(value, t);
                }
                catch (FormatException)
                {
                    result = Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
                }
            }

            return result;
        }

        //        struct Value
        //        {
        //            private Guid? _guid;
        //            private DateTime? _dateTime;
        //            private int? _integer;
        //            private decimal? _dec;
        //            private bool? _boolean;
        //            private string _str;
        //
        //            public Value(Guid guid)
        //            {
        //                _guid = guid;
        //                _dateTime = null;
        //                _integer = null;
        //                _dec = null;
        //                _boolean = null;
        //                _str = null;
        //            }
        //
        //            public Value(DateTime dateTime)
        //            {
        //                _guid = null;
        //                _dateTime = dateTime;
        //                _integer = null;
        //                _dec = null;
        //                _boolean = null;
        //                _str = null;
        //            }
        //
        //            public Value(int integer)
        //            {
        //                _guid = null;
        //                _dateTime = null;
        //                _integer = integer;
        //                _dec = null;
        //                _boolean = null;
        //                _str = null;
        //            }
        //
        //            public Value(decimal dec)
        //            {
        //                _guid = null;
        //                _dateTime = null;
        //                _integer = null;
        //                _dec = dec;
        //                _boolean = null;
        //                _str = null;
        //            }
        //
        //            public Value(bool boolean)
        //            {
        //                _guid = null;
        //                _dateTime = null;
        //                _integer = null;
        //                _dec = null;
        //                _boolean = boolean;
        //                _str = null;
        //            }
        //
        //            public Value(string str)
        //            {
        //                _guid = null;
        //                _dateTime = null;
        //                _integer = null;
        //                _dec = null;
        //                _boolean = null;
        //                _str = str;
        //            }
        //        }
    }
}