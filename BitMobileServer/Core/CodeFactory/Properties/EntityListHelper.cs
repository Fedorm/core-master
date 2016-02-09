using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.AdminApp
{
    public partial class EntityList : EntityListBase
    {
        private CodeFactory.Config config;
        private CodeFactory.Entity entity;

        public EntityList(CodeFactory.Config config, CodeFactory.Entity entity)
        {
            this.config = config;
            this.entity = entity;
        }
    }
}
