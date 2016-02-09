using System;
using System.Collections.Generic;

namespace BitMobile.ValueStack
{
    public class CustomDictionary : Dictionary<String, object>, IIndexedProperty
    {
        public CustomDictionary()
        {
        }

        public void Push(String key, object value)
        {
            this.Add(key, value);
        }

        public new void Add(String key, object value)
        {
            if (this.ContainsKey(key))
                this.Remove(key);
            base.Add(key, value);
        }

        public object Get(String key)
        {
            return GetValue(key);
        }

        public object HasValue(String key)
        {
            return this.ContainsKey(key);
        }

        //---------------------------------------IIndexedProperty

        public object GetValue(String key)
        {
            object result = null;
            if (TryGetValue(key, out result))
                return result;
            else
                return null;
        }

        public bool HasProperty(string propertyName)
        {
            return this.ContainsKey(propertyName);
        }
    }
}
