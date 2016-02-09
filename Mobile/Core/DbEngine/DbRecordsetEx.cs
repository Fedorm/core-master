using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using Mono.Data.Sqlite;

namespace BitMobile.DbEngine
{
    public class DbRecordsetEx : IDbRecordsetEx
    {
        private Dictionary<String, int> columnNames = null;
        private DataTable table;
        private int currentIndex = -1;

        public DbRecordsetEx(DbRecordset rs)
        {
            table = new DataTable("recordset");
            table.Load(rs);

            columnNames = new Dictionary<string, int>();
            for (int i = 0; i < rs.FieldCount; i++)
            {
                String[] arr = rs.GetName(i).Split('.');
                columnNames.Add(arr[arr.Length - 1].ToLower(), i);
            }
        }

        public bool Next()
        {
            currentIndex++;
            return currentIndex < table.Rows.Count;
        }

        public object this[String name]
        {
            get
            {
                return table.Rows[currentIndex][columnNames[name.ToLower()]];
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
                return table.Rows.Count;
            }
        }

        public void Close()
        {
        }

        public System.Data.DataTable GetSchemaTable()
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
        }

        //-----------------------------------------------------IDataRecord

        public int FieldCount
        {
            get
            {
                return columnNames.Count;
            }
        }

        public bool GetBoolean(int i)
        {
            return (bool)table.Rows[currentIndex][i];
        }

        public byte GetByte(int i)
        {
            return (byte)table.Rows[currentIndex][i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)table.Rows[currentIndex][i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)table.Rows[currentIndex][i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)table.Rows[currentIndex][i];
        }

        public double GetDouble(int i)
        {
            return (double)table.Rows[currentIndex][i];
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            return (float)table.Rows[currentIndex][i];
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(table.Rows[currentIndex][i].ToString());
        }

        public short GetInt16(int i)
        {
            return (short)table.Rows[currentIndex][i];
        }

        public int GetInt32(int i)
        {
            return (int)table.Rows[currentIndex][i];
        }

        public long GetInt64(int i)
        {
            return (long)table.Rows[currentIndex][i];
        }

        public string GetName(int i)
        {
            foreach (var item in columnNames)
            {
                if (item.Value == i)
                    return item.Key;
            }
            throw new KeyNotFoundException();
        }

        public int GetOrdinal(string name)
        {
            if (columnNames.ContainsKey(name.ToLower()))
                return columnNames[name.ToLower()];
            else
                return -1;
        }

        public string GetString(int i)
        {
            return (string)table.Rows[currentIndex][i];
        }

        public object GetValue(int i)
        {
            return (table.Rows[currentIndex][i]).DbValue();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return table.Rows[currentIndex][i] == DBNull.Value;
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
            return table.Rows.GetEnumerator();
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
            return this;
        }

        //--------------------------------------IDbRecordsetEx

        public void First()
        {
            currentIndex = -1;
        }

        public int Count()
        {
            return table.Rows.Count;
        }
    }

}
