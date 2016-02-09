using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public class DbRecordset : IDbRecordset
    {
        private SqliteDataReader reader;
        private Dictionary<String, int> columnNames = null;

        public DbRecordset(object obj)
        {
            this.reader = (SqliteDataReader)obj;
        }

        public bool Next()
        {
            return this.reader.Read();
        }

        public object this[String name]
        {
            get
            {
                return this.GetValue(name);
            }
        }

        //----------------------------------------------------IDataReader

        public int Depth
        {
            get 
            {
                return reader.Depth;
            }
        }

        public bool IsClosed
        {
            get 
            {
                return reader.IsClosed; 
            }
        }

        public int RecordsAffected
        {
            get 
            {
                return reader.RecordsAffected;
            }
        }

        public void Close()
        {
            reader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return reader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return reader.NextResult();
        }

        public bool Read()
        {
            return reader.Read();
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        //-----------------------------------------------------IDataRecord

        public int FieldCount
        {
            get 
            {
                return reader.FieldCount;
            }
        }

        public bool GetBoolean(int i)
        {
            return reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public System.Data.IDataReader GetData(int i)
        {
            return (reader as IDataRecord).GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return reader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return reader.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return reader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return reader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return reader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            if (columnNames == null)
            {
                columnNames = new Dictionary<string, int>();
                for (int i = 0; i < this.FieldCount; i++)
                {
                    String[] arr = this.GetName(i).Split('.');
                    columnNames.Add(arr[arr.Length - 1].ToLower(), i);
                }
            }
            //return reader.GetOrdinal(name);
            int idx;
            if (!columnNames.TryGetValue(name.ToLower(), out idx))
                return -1;
            else
                return idx;
        }

        public string GetString(int i)
        {
            return reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return reader.GetValue(i).DbValue();
        }

        public int GetValues(object[] values)
        {
            return reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return reader.IsDBNull(i);
        }

        public object this[int i]
        {
            get 
            { 
                return this.GetValue(i);
            }            
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return reader.GetEnumerator();
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(String propertyName)
        {
            int idx = this.GetOrdinal(propertyName);
            if (idx < 0)
                throw new Exception(String.Format("Recordset does not contain field '{0}'", propertyName));
            else
                return this.GetValue(idx);
        }

        public bool HasProperty(string propertyName)
        {
            return this.GetOrdinal(propertyName) >= 0;
        }

        //--------------------------------------IDbRecordset

        public IDbRecordsetEx Unload()
        {
            return new DbRecordsetEx(this);
        }
    }
}
