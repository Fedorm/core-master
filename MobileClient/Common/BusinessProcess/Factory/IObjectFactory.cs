using System.Xml;
using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;


namespace BitMobile.Common.BusinessProcess.Factory
{
    public interface IObjectFactory
    {
        object Build(IContainer parent, XmlNode node, IValueStack stack);
    }
}