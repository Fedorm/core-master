using BitMobile.Common.Application.Exceptions;

namespace BitMobile.Common.ValueStack
{
    public interface IValueStackContext
    {
        IValueStack CreateValueStack(IExceptionHandler exceptionHandler);
        ICustomDictionary CreateDictionary();
        IIteratorStatus CreateIteratorStatus();
        ICommonData CreateCommonData(string os);
    }
}
