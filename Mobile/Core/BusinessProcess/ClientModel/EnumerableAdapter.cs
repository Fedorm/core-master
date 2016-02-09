using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.ClientModel
{
    /// <summary>
    /// Adapter of extensions methods for JINT
    /// </summary>
    public class EnumerableAdapter : IEnumerable
    {
        IEnumerable<object> _source;
        int _maxRows;

        public EnumerableAdapter(IEnumerable<object> source, int maxRows)
        {
            _source = source;
            _maxRows = maxRows;
        }

        public IEnumerator GetEnumerator()
        {
            int cnt = 0;
            foreach (object item in _source)
            {
                cnt++;
                yield return item;
                if (cnt >= _maxRows)
                    break;
            }
        }

        public int Count()
        {
            return _source.Count();
        }
    }
}