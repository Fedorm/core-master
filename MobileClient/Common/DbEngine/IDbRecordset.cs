using System.Collections;
using System.Data;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;

namespace BitMobile.Common.DbEngine
{
    public interface IDbRecordset : IDataReader, IEnumerable, IIndexedProperty
    {
        bool Next();
        IDbRecordsetEx Unload();
    }
}
