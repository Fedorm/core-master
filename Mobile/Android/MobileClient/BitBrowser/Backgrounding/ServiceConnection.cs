using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Droid.Backgrounding
{
    class ServiceConnection: Java.Lang.Object, IServiceConnection
	{
		public event EventHandler<ServiceConnectedEventArgs> ServiceConnected = delegate {};

        public ServiceBinder Binder { get; private set; }

        public ServiceConnection(ServiceBinder binder)
		{
            Binder = binder;
		}

		public void OnServiceConnected (ComponentName name, IBinder service)
		{
            ServiceBinder serviceBinder = service as ServiceBinder;
			if (serviceBinder != null) {
                this.Binder = serviceBinder;
                this.Binder.IsBound = true;
				this.ServiceConnected(this, new ServiceConnectedEventArgs () { Binder = service } );
			}
		}
			
		public void OnServiceDisconnected (ComponentName name)
		{
			this.Binder.IsBound = false;			
		}
	}
}