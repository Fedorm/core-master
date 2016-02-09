using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class FiltersAzure : FiltersAzureBase
    {
        private CodeFactory.Config config;

        public FiltersAzure(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
