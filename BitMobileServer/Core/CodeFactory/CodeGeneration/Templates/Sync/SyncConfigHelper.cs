using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeFactory.CodeGeneration.Templates.Sync
{
    public partial class SyncConfig : SyncConfigBase
    {
        private CodeFactory.Config config;
        private System.Data.SqlClient.SqlConnectionStringBuilder connection;

        public SyncConfig(CodeFactory.Config config, String connectionString)
        {
            this.config = config;
            this.connection = new System.Data.SqlClient.SqlConnectionStringBuilder();
            this.connection.ConnectionString = connectionString;
        }

    }
}
