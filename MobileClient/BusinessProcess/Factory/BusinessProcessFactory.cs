using System;
using System.IO;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.Factory
{
    public class BusinessProcessFactory : IBusinessProcessFactory
    {
		private static BusinessProcessFactory _factory;

		public static BusinessProcessFactory CreateInstance()
		{
		    return _factory ?? (_factory = new BusinessProcessFactory());
		}

	    private BusinessProcessFactory()
		{
		}

        public IBusinessProcess CreateBusinessProcess(String bpName, IValueStack stack)
		{
			Stream bpData = Application.ApplicationContext.Current.Dal.GetBusinessProcess(bpName);
			var factory = new ObjectFactory();
			return (WorkingProcess.BusinessProcess)factory.CreateObject(stack, bpData);
		}
	}
}

