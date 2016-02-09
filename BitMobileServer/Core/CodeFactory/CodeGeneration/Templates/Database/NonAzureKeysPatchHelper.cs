using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class NonAzureKeysPatch : NonAzureKeysPatchBase
    {
        private CodeFactory.Config config;

        public NonAzureKeysPatch(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
