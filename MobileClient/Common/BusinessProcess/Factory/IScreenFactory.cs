using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.BusinessProcess.Factory
{
    public interface IScreenFactory
    {
        object CreateScreen(string screenName, IValueStack stack, IScreenController controller, object styleCache);
    }
}
