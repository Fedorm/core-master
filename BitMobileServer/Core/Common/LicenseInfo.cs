using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LicenseInfo
    {
        public String Server {get;set;}
        public Guid Id { get; set; }
        public String Name { get; set; }
        public int Qty { get; set; }
        public DateTime ExpireDate { get; set; }

        public override string ToString()
        {
            return string.Format("Server:{0}, Id:{1}, Name:{2}, Qty:{3}, ExpireDate:{4}", Server, Id, Name, Qty, ExpireDate);
        }
    }
}
