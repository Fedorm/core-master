using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptService
{
    public interface IDbRecordset : System.Data.IDataReader, System.Collections.IEnumerable, BitMobile.ValueStack.IIndexedProperty
    {
    }
}
