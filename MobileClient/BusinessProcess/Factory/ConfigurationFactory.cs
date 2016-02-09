using System.IO;
using BitMobile.Application;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.Factory
{
    public class ConfigurationFactory : IConfigurationFactory
    {
		private static ConfigurationFactory _factory;

		public static ConfigurationFactory CreateInstance()
		{
		    return _factory ?? (_factory = new ConfigurationFactory());
		}
        
		public IConfiguration CreateConfiguration(IValueStack stack)
		{
            Stream bpData = ApplicationContext.Current.Dal.GetConfiguration();
			var factory = new ObjectFactory();
            return (IConfiguration)factory.CreateObject(stack, bpData);
		}
	}
}

