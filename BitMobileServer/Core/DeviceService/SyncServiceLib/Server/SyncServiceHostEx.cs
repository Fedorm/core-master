using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Microsoft.Synchronization.Services
{
    [Serializable]
    public class SyncServiceHostEx : SyncServiceHost
    {
        private String name;

        public String Name
        {
            get
            {
                return name;
            }
        }

        public SyncServiceHostEx(String name, Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            this.name = name;
            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new SyncServiceInstanceProvider(serviceType, name));
            }
        }

        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            foreach (var endpoint in this.Description.Endpoints)
            {
                var binding = endpoint.Binding;
                if (binding is System.ServiceModel.Channels.CustomBinding)
                {
                    var web = binding as System.ServiceModel.Channels.CustomBinding;
                    foreach (var e in web.Elements)
                    {
                        if (e is System.ServiceModel.Channels.HttpTransportBindingElement)
                        {
                            var transport = (System.ServiceModel.Channels.HttpTransportBindingElement)e;
                            transport.MaxReceivedMessageSize = 1000000000; //1Gb should enough
                        }
                    }
                }
            }
        }

    }


    public class SyncServiceInstanceProvider: IInstanceProvider, IContractBehavior
    {
        private Type syncServiceType;
        private String name;

        public SyncServiceInstanceProvider(Type syncServiceType, String name)
        {
            this.syncServiceType = syncServiceType;
            this.name = name;
        }

        public object GetInstance(InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            System.Reflection.ConstructorInfo cinfo = syncServiceType.GetConstructor(new Type[] { typeof(String), typeof(System.Net.NetworkCredential) });
            if (cinfo == null)
                throw new Exception(String.Format("Type {0} does not implement String parameter constructor", syncServiceType.ToString()));
            else
            {
                try
                {
                    return cinfo.Invoke(new object[] { name, GetCredentials() });
                }
                catch (Exception e)
                {
                    if (e.InnerException is UnauthorizedAccessException)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                        WebOperationContext.Current.OutgoingResponse.StatusDescription = e.InnerException.Message;
                        return null;
                        //throw new System.Web.HttpException(401, e.Message);
                    }
                    throw e;
                }
            }
        }

        private System.Net.NetworkCredential GetCredentials()
        {
            String authString = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if (authString != null)
            {
                if (authString.StartsWith("Basic "))
                {
                    String[] arr = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authString.Substring(6))).Split(':');
                    return new System.Net.NetworkCredential(arr[0],arr[1]);
                }
            }
            return null;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }


}
