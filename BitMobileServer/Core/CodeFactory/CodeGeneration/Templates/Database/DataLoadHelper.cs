using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Database
{
    public partial class DataLoad : DataLoadBase
    {
        private CodeFactory.Config config;

        public DataLoad(CodeFactory.Config config)
        {
            this.config = config;
        }

    }
}
