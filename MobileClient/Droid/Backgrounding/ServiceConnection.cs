using Android.Content;
using Android.OS;
using System;

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
            var serviceBinder = service as ServiceBinder;
			if (serviceBinder != null) {
                Binder = serviceBinder;
                Binder.IsBound = true;
				ServiceConnected(this, new ServiceConnectedEventArgs { Binder = service } );
			}
		}
			
		public void OnServiceDisconnected (ComponentName name)
		{
			Binder.IsBound = false;			
		}
	}
}