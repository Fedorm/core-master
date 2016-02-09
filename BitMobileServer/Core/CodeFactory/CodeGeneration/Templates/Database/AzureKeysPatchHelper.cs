using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class AzureKeysPatch : AzureKeysPatchBase
    {
        private CodeFactory.Config config;

        public AzureKeysPatch(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
