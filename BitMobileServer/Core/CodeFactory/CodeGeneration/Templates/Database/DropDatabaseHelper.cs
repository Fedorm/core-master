using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class DropDatabase : DropDatabaseBase
    {
        private CodeFactory.Config config;

        public DropDatabase(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
