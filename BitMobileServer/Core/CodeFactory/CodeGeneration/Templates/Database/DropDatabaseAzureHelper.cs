using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class DropDatabaseAzure : DropDatabaseAzureBase
    {
        private CodeFactory.Config config;

        public DropDatabaseAzure(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
