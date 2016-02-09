using System;
using BitMobile.Common.Log;

namespace BitMobile.Common.Application.Exceptions
{
    public interface IExceptionHandler
    {
        void Handle(Exception e);
        void Handle(object report, Action next);
        void HandleNonFatal(Exception e);
        IReport GetReport(bool isFatal, string report);
    }
}
