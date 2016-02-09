using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class DatabaseAdmin : DatabaseAdminBase
    {
        private CodeFactory.Config config;

        public DatabaseAdmin(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}