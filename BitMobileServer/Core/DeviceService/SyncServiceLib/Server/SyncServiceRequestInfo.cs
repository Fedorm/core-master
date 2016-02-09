using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Synchronization.Services
{
    public class SyncServiceRequestInfo
    {
        private DateTime startTime { get; set; }
        private DateTime endTime { get; set; }
        private String ipAddress { get; set; }
        private String deviceId { get; set; }
        private String coreVersion { get; set; }
        private String configName { get; set; }
        private String configVersion { get; set; }
        private String userId { get; set; }
        private String userName { get; set; }
        private String password { get; set; }
    }
}
