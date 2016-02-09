using BitMobile.Utilities.Exceptions;
using System;

namespace BitMobile.Controls
{
    public class ActionHandlerEx
    {
        ValueStack.ValueStack _valueStack;

        String _module;
        String _func;
        object[] _parameters;

        public static bool Busy { get; set; }

        public ActionHandlerEx(string expression, ValueStack.ValueStack valueStack, object sender)
        {
            expression = expression.Trim();

            _valueStack = valueStack;

            if (expression.StartsWith("$") && expression.Contains("("))
                PrepareScriptCall(expression, sender);
            else
                throw new Exception("Incorrect action: " + expression);
        }

        public object Execute()
        {
            if (Busy)
                return null;

            object result = null;
            try
            {
                object[] parametersToExecute = new object[_parameters.Length];

                for (int i = 0; i < _parameters.Length; i++)
                {
                    Func<object> func = _parameters[i] as Func<object>;
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

        void PrepareScriptCall(String expression, object sender)
        {
            int pos1 = expression.IndexOf("(");
            int pos2 = expression.IndexOf(")");
            _module = "";
            _func = expression.Substring(1, pos1 - 1);
            String[] arr = _func.Split('.');
            if (arr.Length > 2)
                throw new Exception(String.Format("Invalid expression '{0}'", expression));
            if (arr.Length == 2)
            {
                _module = arr[0];
                _func = arr[1];
            }

            String[] args = expression.Substring(pos1 + 1, pos2 - pos1 - 1).Split(',');

            _parameters = new object[args.Length + 1];
            _parameters[0] = sender;

            int i = 1;
            foreach (String arg in args)
            {
                if (IsLazy(arg))
                    _parameters[i] = new Func<object>(() => _valueStack.Evaluate(arg));
                else
                    _parameters[i] = _valueStack.Evaluate(arg);
                i++;
            }
        }

        bool IsLazy(String expression)
        {
            bool result = false;
            expression = expression.Trim();

            if (expression.StartsWith("$"))
            {
                String[] parts = expression.Split('.');
                String root = parts[0].Remove(0, 1);

                if (_valueStack.Values.ContainsKey(root))
                {
                    object obj = _valueStack.Values[root];
                    if (obj is BitMobile.Controls.IDataBind)
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