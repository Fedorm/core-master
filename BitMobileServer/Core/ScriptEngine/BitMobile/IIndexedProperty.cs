using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMobile.ValueStack
{
    public interface IIndexedProperty
    {
        object GetValue(String propertyName);
        bool HasProperty(String propertyName);
    }
}

