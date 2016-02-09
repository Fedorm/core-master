using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Code
{
    public partial class ClientConstants : ClientConstantsBase
    {
        private CodeFactory.Config config;

        public ClientConstants(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
