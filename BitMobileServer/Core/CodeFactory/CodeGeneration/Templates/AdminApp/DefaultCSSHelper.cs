using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.AdminApp
{
    public partial class DefaultCSS : DefaultCSSBase
    {
        private CodeFactory.Config config;

        public DefaultCSS(CodeFactory.Config config)
        {
            this.config = config;
        }
    }
}
