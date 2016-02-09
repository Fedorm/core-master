using System;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.DbEngine
{
    public class DbRef : IDbRef, IConvertible, IIndexedProperty
    {
        private const String Suffix = "@ref";
        private readonly String _tableName;
        private Guid _id;
        private IEntity _obj;

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public Guid Guid
        {
            get
            {
                return _id;
            }
        }

        public String TableName
        {
            get
            {
                return _tableName;
            }
        }

        public bool HasCache { get { return _obj != null; } }

        internal DbRef(String tableName, Guid id)
        {
            _tableName = tableName;
            _id = id;
        }

        public static DbRef CreateInstance(String tableName, Guid id)
        {
            return Database.Current.Cache.GetRef(tableName, id);
        }

        public IEntity GetObject()
        {
            if (_obj == null)
            {
                if (_id != Guid.Empty)
                    _obj = Database.Current.Cache.GetObject(this);
            }
            return _obj;
        }

        public object LoadObject()
        {
            _obj = Database.Current.Cache.GetObject(this);
            return _obj;
        }

        public DbFieldMetadata Metadata()
        {
            return new DbFieldMetadata(_tableName);
        }

        public static DbRef FromString(String s)
        {
            int pos1 = s.IndexOf('[');
            int pos2 = s.IndexOf(']');
            String tableName = s.Substring(pos1 + 1, pos2 - pos1 - 1);
            if (String.IsNullOrEmpty(tableName))
                throw new InvalidOperationException(String.Format("Unable to create DbRef, invalid reference string '{0}'", s));
            var guid = new Guid(s.Substring(pos2 + 2, s.Length - pos2 - 2));

            return CreateInstance(tableName, guid);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is DbRef))
                return false;
            return _id.Equals(((DbRef)obj)._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool CheckIsRef(String s)
        {
            // this bullshit to increase performance
            if (!string.IsNullOrEmpty(s) && s.Length > 4)
                return s[0] == '@' && s[1] == 'r' && s[2] == 'e' && s[3] == 'f';
            return false;
        }

        public override string ToString()
        {
            return CreateKey(_tableName, _id.ToString());
        }

        public string GetString()
        {
            return ToString();
        }

        public object this[String name]
        {
            get
            {
                IEntity o = GetObject();
                if (o.HasProperty(name))
                    return o.GetValue(name);
                throw new Exception(String.Format("Invalid property name '{0}'", name));
            }
            set
            {
                IEntity o = GetObject();
                if (o.HasProperty(name))
                    o.SetValue(name, value);
                else
                    throw new Exception(String.Format("Invalid property name '{0}'", name));

            }
        }

        private static string CreateKey(String tableName, String id)
        {
            return String.Format("{0}[{1}]:{2}", Suffix, tableName, id);
        }

        private static string CreateKey(String tableName, Guid id)
        {
            return CreateKey(tableName, id.ToString());
        }

        //---------------------------------------IConvertable----------------------------------------------

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == GetType())
                return this;
            throw new InvalidCastException(String.Format("Cant convert '{0}' to @Ref[{1}]", conversionType.FullName, _tableName));
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public bool EmptyRef()
        {
            return _id.Equals(Guid.Empty);
        }

        public bool IsNew()
        {
            object obj = GetObject();
            if (obj == null)
                return false;
            return (obj as ISqliteEntity).IsNew();
        }

        public bool IsNewInternal()
        {
            if (_obj == null)
                return false;
            return IsNew();
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(string name)
        {
            return this[name];
        }

        public bool HasProperty(string propertyName)
        {
            IEntity obj = GetObject();
            if (obj == null)
                return false;
            return obj.HasProperty(propertyName);
        }
    }
}
