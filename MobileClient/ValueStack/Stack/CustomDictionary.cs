using System;
using System.Collections.Generic;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    public class CustomDictionary : Dictionary<String, object>, ICustomDictionary
    {
        public CustomDictionary()
        {
        }

        public void Push(String key, object value)
        {
            Add(key, value);
        }

        public new void Add(String key, object value)
        {
            if (ContainsKey(key))
                Remove(key);
            base.Add(key, value);
        }

        public object Get(String key)
        {
            return GetValue(key);
        }

        public object HasValue(String key)
        {
            return ContainsKey(key);
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(String key)
        {
            object result;
            if (TryGetValue(key, out result))
                return result;
            return null;
        }

        public bool HasProperty(string propertyName)
        {
            return ContainsKey(propertyName);
        }
    }
}

