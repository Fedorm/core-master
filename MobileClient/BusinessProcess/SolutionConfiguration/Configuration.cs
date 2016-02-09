using System;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.Controls;

namespace BitMobile.BusinessProcess.SolutionConfiguration
{
    [MarkupElement(MarkupElementAttribute.ConfigurationNamespace, "Configuration")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    public class Configuration : IConfiguration, IContainer
    {
        public Configuration()
        {
            Style = new Style();
            Script = new Script();
        }

        public IBusinessProcess BusinessProcess { get; set; }

        public IScript Script { get; set; }

        public IStyle Style { get; set; }

        public void AddChild(object obj)
        {
            // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
            if (obj is BusinessProcess)
                BusinessProcess = (BusinessProcess)obj;
            if (obj is Style)
                Style = (IStyle)obj;
            if (obj is Script)
                Script = (IScript)obj;
            // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
        }

        public object[] Controls
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object GetControl(int index)
        {
            throw new NotImplementedException();
        }
    }
}
