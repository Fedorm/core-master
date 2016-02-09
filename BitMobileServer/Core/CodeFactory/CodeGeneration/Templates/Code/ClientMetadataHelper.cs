using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Code
{
    public partial class ClientMetadata : ClientMetadataBase
    {
        private CodeFactory.Config config;

        public ClientMetadata(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
