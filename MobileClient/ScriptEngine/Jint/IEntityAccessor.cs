using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jint
{
    public interface IEntityAccessor
    {
        object GetValue(object obj, string column);
        void SetValue(object obj, string column, object value);
        bool TryGetValue(object obj, string column, out object value);
        bool TrySetValue(object obj, string column, object value);
    }
}