using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class DomainManager
    {
        private static Dictionary<string, AppDomain> m_domains = new Dictionary<string, AppDomain>();

        public static T GetProxy<T>(String domainName)
        {
            return (T)GetProxy(domainName, System.Reflection.Assembly.GetExecutingAssembly().FullName, typeof(T).FullName);
        }

        public static T GetProxy<T>(String domainName, String assemblyName)
        {
            return (T)GetProxy(domainName, assemblyName, typeof(T).FullName);
        }

        public static object GetProxy(String domainName, String assemblyName, String typeName, AppDomainInitializer initializer = null, string[] initializerArguments = null)
        {
            AppDomain domain;
            if (!m_domains.TryGetValue(domainName, out domain))
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                setup.PrivateBinPath = "bin";
                setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                setup.AppDomainInitializer = initializer;
                setup.AppDomainInitializerArguments = initializerArguments;
                Evidence evidence = AppDomain.CurrentDomain.Evidence;

                domain = AppDomain.CreateDomain(domainName, evidence, setup);

                m_domains.Add(domainName, domain);
            }

            return domain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        public static void UnloadDomain(String name)
        {
            if (m_domains.ContainsKey(name))
            {
                AppDomain domainToUnload = m_domains[name];
                if (!domainToUnload.Equals(AppDomain.CurrentDomain))
                {
                    System.AppDomain.Unload(domainToUnload);
                    m_domains.Remove(name);
                }
            }
        }

    }
}
