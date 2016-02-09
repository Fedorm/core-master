using System;

using BitMobile.Configuration;
using BitMobile.ValueStack;

namespace BitMobile.Factory
{
	public class ConfigurationFactory
	{
		private static ConfigurationFactory factory = null;

		public static ConfigurationFactory CreateInstance()
		{
			if(factory == null)
				factory = new ConfigurationFactory();
			return factory;
		}

		private ConfigurationFactory()
		{
		}

		public Configuration.Configuration CreateConfiguration(ValueStack.ValueStack stack)
		{
            System.IO.Stream bpData = BitMobile.Application.ApplicationContext.Context.DAL.GetConfiguration();
			ObjectFactory factory = new ObjectFactory();
            return (Configuration.Configuration)factory.CreateObject(stack, bpData);
		}
	}
}

