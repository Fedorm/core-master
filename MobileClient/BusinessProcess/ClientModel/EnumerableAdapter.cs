using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BitMobile.BusinessProcess.ClientModel
{
    /// <summary>
    /// Adapter of extensions methods for JINT
    /// </summary>
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class EnumerableAdapter : IEnumerable
    {
        private readonly IEnumerable<object> _source;
        private readonly int _maxRows;

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