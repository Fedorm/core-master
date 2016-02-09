using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;

using System.Threading;

namespace SyncRole
{
    public class FtpHelper
    {
        private String solutionFolder;
        private String passiveAddress;
        private int port;

        public FtpHelper(String solutionFolder, String passiveAddress, int port)
        {
            this.solutionFolder = solutionFolder;
            this.passiveAddress = passiveAddress;
            this.port = port;
        }

        public void Start()
        {
            new Thread(Execute).Start();
        }

        private void Execute()
        {
            FtpService.FtpServer ftpServer = new FtpService.FtpServer(solutionFolder, port);
            ftpServer.PassiveAddress = passiveAddress;
            ftpServer.Start();
        }
    }
}