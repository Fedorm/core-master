using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class Database : DatabaseBase
    {
        private CodeFactory.Config config;

        public Database(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
