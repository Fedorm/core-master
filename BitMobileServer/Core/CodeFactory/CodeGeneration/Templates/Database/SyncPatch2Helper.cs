using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class SyncPatch2 : SyncPatch2Base
    {
        private CodeFactory.Config config;

        public SyncPatch2(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
