using System;
using System.Collections.Generic;
using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.Controls
{
    public abstract class ActionHandlerAbstract
    {
        private readonly IValueStack _valueStack;
        private readonly IList<object> _parameters;
        private string _func;

        protected ActionHandlerAbstract(string expression, IValueStack valueStack, IList<object> parameters)
        {
            Expression = expression;

            _valueStack = valueStack;
            _parameters = parameters;

            if (expression.StartsWith("$") && expression.Contains("("))
                PrepareScriptCall(expression);
            else
                throw new Exception("Incorrect action: " + expression);
        }

        public string Expression { get; }

        public object Execute()
        {
            if (Application.Controls.ControlsContext.Current.ActionHandlerLocker.Locked)
                return null;

            object result = null;
            try
            {
                var parametersToExecute = new object[_parameters.Count];

                for (int i = 0; i < _parameters.Count; i++)
                {
                    var func = _parameters[i] as Func<object>;
                    if (func != null)
                        parametersToExecute[i] = func.Invoke();
                    else
                        parametersToExecute[i] = _parameters[i];
                }

                result = _valueStack.CallScript(_func, parametersToExecute);
            }
            catch (Exception e)
            {
                _valueStack.ExceptionHandler.Handle(e);
            }
            return result;
        }

        private void PrepareScriptCall(String expression)
        {
            // parse function name
            int index = -1;
            int start = 0;
            while (++index < expression.Length)
            {
                char c = expression[index];
                if (c == '$' || c == '.'/*backward compatibility: $Workflow.DoBack()*/)
                    start = index + 1;
                else if (c == '(')
                    break;
            }
            _func = expression.Substring(start, index - start);

            // parse agrs
            start = index + 1;
            while (++index < expression.Length)
            {
                char c = expression[index];
                if (c == ')' || c == ',')
                {
                    if (start != index)
                    {
                        string arg = expression.Substring(start, index - start);
                        _parameters.Add(IsLazy(arg) ? new Func<object>(() => _valueStack.Evaluate(arg)) : _valueStack.Evaluate(arg));
                        start = index + 1;
                    }

                    if (c == ')')
                        break;
                }
            }
        }

        bool IsLazy(string expression)
        {
            bool result = false;

            int index = -1;

            while (++index < expression.Length && expression[index] == ' ') { } // trim

            if (expression[index] == '$')
            {
                int start = index + 1;

                // parse variable name
                while (++index < expression.Length && expression[index] != '.') { }
                string root = expression.Substring(start, index - start);

                object obj;
                if (_valueStack.Values.TryGetValue(root, out obj))
                {
                    if (obj is IDataBind)
                        result = true;
                }
                else
                    // If expression cannot find item in ValueStack, it can appear later
                    result = true;
            }
            return result;
        }
    }
}