using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace ScriptService
{
    public class DbRecordset : IDbRecordset
    {
        private DataTable table;
        private Dictionary<String, int> columnNames = null;
        private int currentIndex = -1;

        public DbRecordset(object obj)
        {
            this.table = (DataTable)obj;
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
                return this.GetValue(name);
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
            return this.Next();
        }

        public void Dispose()
        {
        }

        //-----------------------------------------------------IDataRecord

        public int FieldCount
        {
            get
            {
                return table.Columns.Count;
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
            return 0;
        }

        public char GetChar(int i)
        {
            return (char)table.Rows[currentIndex][i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return 0;
        }

        public System.Data.IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return table.Columns[i].DataType.Name;
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)table.Rows[currentIndex][i];
        }

        public decimal GetDecimal(int i)
        {
            return (Decimal)table.Rows[currentIndex][i];
        }

        public double GetDouble(int i)
        {
            return (Double)table.Rows[currentIndex][i];
        }

        public Type GetFieldType(int i)
        {
            return table.Columns[i].DataType;
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
            return table.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            int i = -1;
            foreach (DataColumn c in table.Columns)
            {
                i++;
                if (name.ToLower().Equals(c.ColumnName.ToLower()))
                    return i;
            }
            return i;
        }

        public string GetString(int i)
        {
            return (string)table.Rows[currentIndex][i];
        }

        public object GetValue(int i)
        {
            return table.Rows[currentIndex][i];
        }

        public int GetValues(object[] values)
        {
            return 0;
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
            while (this.Next())
                yield return this;
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

    }
}
