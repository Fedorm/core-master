using System;
using Android.OS;

namespace BitMobile.Droid.Backgrounding
{	
	public class ServiceBinder : Binder
	{
        public BaseService Service { get; private set; }
        
		public bool IsBound { get; set; }
			
        public ServiceBinder(BaseService service)
		{
            this.Service = service;
		}
	}
}

