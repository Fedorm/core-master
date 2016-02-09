using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Code
{
    public partial class Client : ClientBase
    {
        private CodeFactory.Config config;

        public Client(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
