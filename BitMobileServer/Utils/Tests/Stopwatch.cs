using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Stopwatch
    {
        System.Diagnostics.Stopwatch _current = new System.Diagnostics.Stopwatch();

        public void Start()
        {
            _current.Reset();
            _current.Start();            
        }

        public TimeSpan Elapsed
        {
            get
            {
                return _current.Elapsed;
            }
        }

        public TimeSpan Stop()
        {
            _current.Stop();
            return _current.Elapsed;
        }
    }
}
