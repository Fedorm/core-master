using System;

namespace BitMobile.Application
{
	public class ApplicationContext
	{
		private static IApplicationContext context;

		public static IApplicationContext Context {
			get {
				return context;
			}
		}

		public static void InitContext(IApplicationContext ctx)
		{
			context = ctx;
		}
	}
}

