using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UnsupportedCoreException : System.Exception
    {
        public UnsupportedCoreException(String message)
            : base(message)
        {
        }
    }

    public class LicenseException : System.Exception
    {
        public LicenseException(String message)
            : base(message)
        {
        }
    }

    public class ConflictVersionException : System.Exception
    {
        public ConflictVersionException(String message)
            : base(message)
        {
        }
    }

}
