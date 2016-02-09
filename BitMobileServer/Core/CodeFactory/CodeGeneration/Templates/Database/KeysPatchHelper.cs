using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class KeysPatch : KeysPatchBase
    {
        private CodeFactory.Config config;

        public KeysPatch(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
