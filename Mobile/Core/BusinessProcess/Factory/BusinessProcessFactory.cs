using System;

using BitMobile.BusinessProcess;
using BitMobile.ValueStack;

namespace BitMobile.Factory
{
	public class BusinessProcessFactory
	{
		private static BusinessProcessFactory factory = null;

		public static BusinessProcessFactory CreateInstance()
		{
			if(factory == null)
				factory = new BusinessProcessFactory();
			return factory;
		}

		private BusinessProcessFactory()
		{
		}

		public BusinessProcess.BusinessProcess CreateBusinessProcess(String bpName, ValueStack.ValueStack stack)
		{
			System.IO.Stream bpData = BitMobile.Application.ApplicationContext.Context.DAL.GetBusinessProcess(bpName);
			ObjectFactory factory = new ObjectFactory();
			return (BusinessProcess.BusinessProcess)factory.CreateObject(stack, bpData);
		}
	}
}

