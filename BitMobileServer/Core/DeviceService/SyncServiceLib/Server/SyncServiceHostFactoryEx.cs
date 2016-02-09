using System;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.Security.Policy;

using System.Threading;

namespace Microsoft.Synchronization.Services
{
    public class SyncServiceHostFactoryEx : SyncServiceHostFactory
    {
        private String _name;
        private bool isActive = true;
        private SyncServiceHostEx host = null;

        public SyncServiceHostFactoryEx(String name, bool registerKiller = false)
            :base()
        {
            this._name = name;
            if (registerKiller)
                CreateHostKiller();
        }

        private void CreateHostKiller()
        {
            System.Threading.Thread t = new System.Threading.Thread(ThreadFunc);
            t.Start();
        }

        private void ThreadFunc()
        {
            try
            {
                EventWaitHandle[] handles = new EventWaitHandle[2];
                handles[0] = new EventWaitHandle(false, EventResetMode.AutoReset, Common.Solution.HostKillerName(_name, "start"));
                handles[1] = new EventWaitHandle(false, EventResetMode.AutoReset, Common.Solution.HostKillerName(_name, "stop"));
                while (true)
                {
                    int idx = EventWaitHandle.WaitAny(handles);
                    switch (idx)
                    {
                        case 0: //start
                            this.isActive = true;
                            break;
                        case 1: //stop
                            this.isActive = false;
                            if (host != null)
                            {
                                host.Abort();
                                host = null;
                                Common.DomainManager.UnloadDomain(_name);
                            }
                            break;
                    }
                }
            }
            catch(Exception)
            {
            }
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            if (isActive)
            {
                host = new SyncServiceHostEx(_name, serviceType, baseAddresses);
                return host;
            }
            else
                throw new Exception("Service host is blocked");
        }

    }

}
