using System;
using System.Collections.Generic;
using System.Data;
using BitMobile.Common.DbEngine;

namespace BitMobile.DbEngine
{
    public class DbRecordsetEx : IDbRecordsetEx
    {
        private readonly Dictionary<String, int> _columnNames;
        private readonly DataTable _table;
        private int _currentIndex = -1;

        public DbRecordsetEx(DbRecordset rs)
        {
            _table = new DataTable("recordset");
            _table.Load(rs);

            _columnNames = new Dictionary<string, int>();
            for (int i = 0; i < rs.FieldCount; i++)
            {
                String[] arr = rs.GetName(i).Split('.');
                _columnNames.Add(arr[arr.Length - 1].ToLower(), i);
            }
        }

        public bool Next()
        {
            _currentIndex++;
            return _currentIndex < _table.Rows.Count;
        }

        public object this[String name]
        {
            get
            {
                return _table.Rows[_currentIndex][_columnNames[name.ToLower()]];
            }
        }

        //----------------------------------------------------IDataReader

        public int Depth
        {
            get
            {
                return 0;
            }
        }

        public bool IsClosed
        {
            get
            {
                return false;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return _table.Rows.Count;
            }
        }

        public void Close()
        {
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return Next();
        }

        public void Dispose()
        {
            _table.Dispose();
        }

        //-----------------------------------------------------IDataRecord

        public int FieldCount
        {
            get
            {
                return _columnNames.Count;
            }
        }

        public bool GetBoolean(int i)
        {
            return (bool)_table.Rows[_currentIndex][i];
        }

        public byte GetByte(int i)
        {
            return (byte)_table.Rows[_currentIndex][i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)_table.Rows[_currentIndex][i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)_table.Rows[_currentIndex][i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)_table.Rows[_currentIndex][i];
        }

        public double GetDouble(int i)
        {
            return (double)_table.Rows[_currentIndex][i];
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            return (float)_table.Rows[_currentIndex][i];
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(_table.Rows[_currentIndex][i].ToString());
        }

        public short GetInt16(int i)
        {
            return (short)_table.Rows[_currentIndex][i];
        }

        public int GetInt32(int i)
        {
            return (int)_table.Rows[_currentIndex][i];
        }

        public long GetInt64(int i)
        {
            return (long)_table.Rows[_currentIndex][i];
        }

        public string GetName(int i)
        {
            foreach (var item in _columnNames)
            {
                if (item.Value == i)
                    return item.Key;
            }
            throw new KeyNotFoundException();
        }

        public int GetOrdinal(string name)
        {
            if (_columnNames.ContainsKey(name.ToLower()))
                return _columnNames[name.ToLower()];
            return -1;
        }

        public string GetString(int i)
        {
            return (string)_table.Rows[_currentIndex][i];
        }

        public object GetValue(int i)
        {
            return (_table.Rows[_currentIndex][i]).DbValue();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return _table.Rows[_currentIndex][i] == DBNull.Value;
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
            return _table.Rows.GetEnumerator();
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(String propertyName)
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
            return this;
        }

        //--------------------------------------IDbRecordsetEx

        public void First()
        {
            _currentIndex = -1;
        }

        public int Count()
        {
            return _table.Rows.Count;
        }
    }

}
