using System.Collections.Generic;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "GlobalModules")]
    public class GlobalModules : IGlobalModules, IContainer
    {
        private readonly List<Module> _modules;

        public GlobalModules()
        {
            _modules = new List<Module>();
        }

        public void AddChild(object obj)
        {
            _modules.Add((Module)obj);
        }

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return _modules.ToArray();
            }
        }


        public object GetControl(int index)
        {
            return _modules[index];
        }
    }

    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Module")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Module
    {
        public string Name { get; set; }

        public string File { get; set; }
    }
}

