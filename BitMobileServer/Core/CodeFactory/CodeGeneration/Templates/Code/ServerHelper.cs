using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Code
{
    public partial class Server : ServerBase
    {
        private CodeFactory.Config config;

        public Server(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
