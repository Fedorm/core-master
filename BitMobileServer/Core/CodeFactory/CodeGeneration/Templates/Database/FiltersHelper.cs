using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class Filters : FiltersBase
    {
        private CodeFactory.Config config;

        public Filters(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
