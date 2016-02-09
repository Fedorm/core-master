using System;
using System.Collections;
using System.Collections.Generic;
using BenchmarkXamarin.Core;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.Controls;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.ValueStack;
using BitMobile.ExpressionEvaluator;

namespace BitMobile.Benchmarks
{
    // ReSharper disable UnusedMember.Global
    class EvaluatorBenchmark
    {
        private readonly Evaluator _evaluator;

        public EvaluatorBenchmark()
        {
            _evaluator = new Evaluator(new ValueStackStub(), new TranslyatorStub());
            _evaluator.SetController(new ControllerStub());
        }

        [Benchmark]
        public void Evaluate_Simple()
        {
            _evaluator.Evaluate("$value");
        }

        [Benchmark]
        public void Evaluate_Formated()
        {
            _evaluator.Evaluate("#price2# {$FormatValue($item.Price)}  #stock#: {$item.CommonStock}  #brand#: {$item.Brand}");
        }

        [Benchmark]
        public void Evaluate_FunctionExecution()
        {
            _evaluator.Evaluate("$GetQuickOrder($item.Id, $item.Price, pack{$index}, editText{$index}, textView{$index}, $item.RecOrder, $item.UnitId, $item.RecUnit)");
        }

        [Benchmark]
        public void Evaluate_FunctionExecutionEx()
        {
            _evaluator.Evaluate("$CreateOrderItem(editText{$index}, textView{$index}, pack{$index}, $item.Id, $item.Price, swipe_layout{$index}, $item.RecOrder, $item.UnitId)");
        }

        [Benchmark]
        public void Evaluate_FunctionExecutionEmpty()
        {
            _evaluator.Evaluate("$CreateOrderItem()");
        }

        [Benchmark]
        public void Evaluate_FunctionExecutionLong()
        {
            _evaluator.Evaluate("$CreateOrderItem($longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong)");
        }

        [Benchmark]
        public void Evaluate_VariableMemebers()
        {
            _evaluator.Evaluate("$item.Price");
        }

        class ControllerStub : IExternalFunction
        {
            public object CallFunction(string functionName, object[] parameters)
            {
                return functionName;
            }

            public object CallFunctionNoException(string functionName, object[] parameters)
            {
                throw new NotImplementedException();
            }

            public object CallVariable(string varName)
            {
                return varName;
            }
        }

        class TranslyatorStub : ITranslator
        {
            public string TranslateByKey(string key)
            {
                return key;
            }
        }

        class ValueStackStub : IValueStack
        {
            public ValueStackStub()
            {
                Values = new DictionaryStub();
            }

            private class DictionaryStub : IDictionary<string, object>
            {
                public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
                {
                    throw new NotImplementedException();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public void Add(KeyValuePair<string, object> item)
                {
                    throw new NotImplementedException();
                }

                public void Clear()
                {
                    throw new NotImplementedException();
                }

                public bool Contains(KeyValuePair<string, object> item)
                {
                    throw new NotImplementedException();
                }

                public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
                {
                    throw new NotImplementedException();
                }

                public bool Remove(KeyValuePair<string, object> item)
                {
                    throw new NotImplementedException();
                }

                public int Count { get; private set; }
                public bool IsReadOnly { get; private set; }
                public bool ContainsKey(string key)
                {
                    throw new NotImplementedException();
                }

                public void Add(string key, object value)
                {
                    throw new NotImplementedException();
                }

                public bool Remove(string key)
                {
                    throw new NotImplementedException();
                }

                public bool TryGetValue(string key, out object value)
                {
                    value = this;
                    return true;
                }

                public object this[string key]
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public ICollection<string> Keys { get; private set; }
                public ICollection<object> Values { get; private set; }
            }

            public IExceptionHandler ExceptionHandler { get; private set; }
            public IDictionary<string, object> Values { get; private set; }
            public IDictionary<string, IPersistable> Persistables { get; private set; }
            public void Push(string name, object value)
            {
                throw new NotImplementedException();
            }

            public object Pull(string name)
            {
                throw new NotImplementedException();
            }

            public object Peek(string name)
            {
                throw new NotImplementedException();
            }

            public bool BooleanExpression(string expression)
            {
                throw new NotImplementedException();
            }

            public void PrepareScriptCall(string expression, out string module, out string func, out object[] parameters)
            {
                throw new NotImplementedException();
            }

            public object CallScript(string functionName, object[] parameters)
            {
                throw new NotImplementedException();
            }

            public object TryCallScript(string functionName, params object[] parameters)
            {
                throw new NotImplementedException();
            }

            public object Evaluate(string expression, Type type = null, bool canString = true)
            {
                throw new NotImplementedException();
            }

            public void Evaluate(string expression, out object obj, out string propertyName)
            {
                throw new NotImplementedException();
            }

            public void SetCurrentController(IScreenController controller)
            {
                throw new NotImplementedException();
            }
        }
    }
}