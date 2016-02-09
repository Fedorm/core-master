using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.AdminApp
{
    public partial class EntityCatalog : EntityCatalogBase
    {
        private CodeFactory.Config config;
        private CodeFactory.Entity entity;

        public EntityCatalog(CodeFactory.Config config, CodeFactory.Entity entity)
        {
            this.config = config;
            this.entity = entity;
        }
    }
}
