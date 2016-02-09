using System;
using Android.OS;

namespace BitMobile.Droid.Backgrounding
{
	public class ServiceConnectedEventArgs : EventArgs
	{
		public IBinder Binder { get; set; }
	}
}