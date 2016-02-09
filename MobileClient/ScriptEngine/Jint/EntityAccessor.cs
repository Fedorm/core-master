using System;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;

namespace Jint
{
    class EntityAccessor : IEntityAccessor
    {
        public object GetValue(object obj, string column)
        {
            object result;
            if (TryGetValue(obj, column, out result))
                return result;
            throw new Exception(string.Format("Column {0} does not exists in {1}", column, obj));
        }

        public void SetValue(object obj, string column, object value)
        {
            if (!TrySetValue(obj, column, value))
                throw new Exception(string.Format("Column {0} does not exists in {1}", column, obj));
        }

        public bool TryGetValue(object obj, string column, out object value)
        {
            var indexedProperty = obj as IIndexedProperty;
            if (indexedProperty != null && indexedProperty.HasProperty(column))
            {
                value = indexedProperty.GetValue(column);
                return true;
            }
            value = null;
            return false;
        }

        public bool TrySetValue(object obj, string column, object value)
        {
            var entity = obj as IEntity;
            if (entity != null && entity.HasProperty(column))
            {
                entity.SetValue(column, value);
                return true;
            }
            return false;
        }
    }
}
