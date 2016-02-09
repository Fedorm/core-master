using System;
using System.Collections.Generic;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.Controls;
using BitMobile.Common.ExpressionEvaluator;
using BitMobile.Common.ValueStack;

namespace BitMobile.ValueStack.Stack
{
    class ValueStack : IValueStack
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IEvaluator _evaluator;

        public ValueStack(IExceptionHandler handler, IExpressionContext expressionContext)
        {
            Values = new CustomDictionary();
            _exceptionHandler = handler;
            _evaluator = expressionContext.CreateEvaluator(this);
            Persistables = new Dictionary<string, IPersistable>();
        }

        public IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler; }
        }

        public IDictionary<string, object> Values { get; private set; }

        public IDictionary<string, IPersistable> Persistables { get; private set; }

        public void Push(String name, object value)
        {
            String fullName = name;
            String[] parts = name.Split('.');
            IDictionary<String, object> dict = Values;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                String part = parts[i];
                if (dict.ContainsKey(part))
                {
                    var dictionary = dict[part] as Dictionary<string, object>;
                    if (dictionary != null)
                    {
                        dict = dictionary;
                        continue;
                    }
                }
                throw new Exception("Invalid variable expression: " + fullName);
            }

            if (value != null)
                if (value.Equals("null"))
                    value = null;

            name = parts[parts.Length - 1];
            if (dict.ContainsKey(name))
                dict.Remove(name);
            dict.Add(name, value);
        }

        public object Pull(String name)
        {
            object result;

            if (Values.TryGetValue(name, out result))
                Values.Remove(name);

            return result;
        }

        public object Peek(string name)
        {
            object result = null;

            if (Values.ContainsKey(name))
                result = Values[name];

            return result;
        }

        public bool BooleanExpression(string expression)
        {
            return _evaluator.EvaluateBooleanExpression(expression);
        }

        public object CallScript(String functionName, object[] parameters)
        {
            var func = (IExternalFunction)Values["controller"];
            return func.CallFunction(functionName, parameters);
        }

        public object TryCallScript(String functionName, params object[] parameters)
        {
            object controller;
            if (Values.TryGetValue("controller", out controller))
                return ((IExternalFunction)controller).CallFunctionNoException(functionName, parameters);
            return null;
        }

        public object Evaluate(string expression, Type type = null, bool canString = true)
        {
            return _evaluator.Evaluate(expression, type);
        }

        public void Evaluate(String expression, out object obj, out String propertyName)
        {
            expression = expression.Trim();

            if (expression.StartsWith("$"))
            {
                String[] parts = expression.Split('.');

                if (parts.Length < 2)
                    throw new Exception(String.Format("Invalid expression: {0}", expression));

                propertyName = parts[parts.Length - 1];
                obj = Evaluate(expression.Substring(0, expression.Length - propertyName.Length - 1));
            }
            else
            {
                throw new Exception("Evaluate expression error - constant is not allowed: " + expression);
            }
        }

        public void SetCurrentController(IScreenController controller)
        {
            _evaluator.SetController(controller);
        }
    }
}