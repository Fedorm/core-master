using System;
using System.Collections.Generic;

namespace BitMobile.Common.ValueStack
{
    public interface ICustomDictionary : IDictionary<string, object>, IIndexedProperty
    {
        void Push(String key, object value);
    }
}
