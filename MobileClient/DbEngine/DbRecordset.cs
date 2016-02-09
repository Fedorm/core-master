using System;
using System.Collections.Generic;
using System.Data;
using BitMobile.Common.DbEngine;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public class DbRecordset : IDbRecordset
    {
        private readonly SqliteDataReader _reader;
        private Dictionary<String, int> _columnNames;

        public DbRecordset(object obj)
        {
            _reader = (SqliteDataReader)obj;
        }

        public bool Next()
        {
            return _reader.Read();            
        }

        public object this[String name]
        {
            get
            {
                return GetValue(name);
            }
        }

        //----------------------------------------------------IDataReader

        public int Depth
        {
            get
            {
                return _reader.Depth;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _reader.IsClosed;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return _reader.RecordsAffected;
            }
        }

        public void Close()
        {
            _reader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return _reader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return _reader.NextResult();
        }

        public bool Read()
        {
            return _reader.Read();
        }

        public void Dispose()
        {
            //todo: you shouldn't execute Dispose in finalizer, because IOS application will crash in release mode.
            _reader.Dispose();
        }

        //-----------------------------------------------------IDataRecord

        public int FieldCount
        {
            get
            {
                return _reader.FieldCount;
            }
        }

        public bool GetBoolean(int i)
        {
            return _reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return (_reader as IDataRecord).GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return _reader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return _reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return _reader.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return _reader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return _reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return _reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _reader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return _reader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            if (_columnNames == null)
            {
                _columnNames = new Dictionary<string, int>();
                for (int i = 0; i < FieldCount; i++)
                {
                    string[] arr = GetName(i).Split('.');
                    _columnNames.Add(arr[arr.Length - 1], i);
                }
            }

            int idx;
            if (!_columnNames.TryGetValue(name, out idx))
            {
                foreach (var column in _columnNames)
                    if (column.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return column.Value;
                return -1;
            }
                
            return idx;
        }

        public string GetString(int i)
        {
            return _reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return _reader.GetValue(i).DbValue();
        }

        public int GetValues(object[] values)
        {
            return _reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return _reader.IsDBNull(i);
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return _reader.GetEnumerator();
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(string propertyName)
        {
            int idx = GetOrdinal(propertyName);
            if (idx < 0)
                throw new Exception(String.Format("Recordset does not contain field '{0}'", propertyName));
            
            return GetValue(idx);
        }

        public bool HasProperty(string propertyName)
        {
            return GetOrdinal(propertyName) >= 0;
        }

        //--------------------------------------IDbRecordset

        public IDbRecordsetEx Unload()
        {
            return new DbRecordsetEx(this);
        }
    }
}
