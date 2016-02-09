using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.AdminApp
{
    public partial class Main : MainBase
    {
        private CodeFactory.Config config;

        public Main(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
