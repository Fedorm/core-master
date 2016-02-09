using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class CreateDatabase : CreateDatabaseBase
    {
        private CodeFactory.Config config;

        public CreateDatabase(CodeFactory.Config config)
        {
            this.config = config;
        }

    }

    public partial class CreateDatabaseAzure : CreateDatabaseAzureBase
    {
        private CodeFactory.Config config;

        public CreateDatabaseAzure(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
