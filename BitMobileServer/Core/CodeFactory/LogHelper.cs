using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory
{
    public static class LogHelper
    {
        public static void Log(this Common.Solution solution, String message)
        {
            Common.Solution.Log(solution.Name, "admin", message);
        }
    }
}
