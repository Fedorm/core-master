using System;
using System.Collections.Generic;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.Controls;

namespace BitMobile.Common.ValueStack
{
    public interface IValueStack
    {
        IExceptionHandler ExceptionHandler { get; }
        IDictionary<String, object> Values { get; }
        IDictionary<string, IPersistable> Persistables { get; }
        void Push(String name, object value);
        object Pull(String name);
        object Peek(string name);
        bool BooleanExpression(String expression);
        object CallScript(String functionName, object[] parameters);
        object TryCallScript(String functionName, params object[] parameters);
        object Evaluate(String expression, Type type = null, bool canString = true);
        void Evaluate(String expression, out object obj, out String propertyName);
        void SetCurrentController(IScreenController controller);
    }
}
