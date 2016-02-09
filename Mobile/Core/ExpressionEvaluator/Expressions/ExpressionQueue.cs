using System.Collections.Generic;

namespace BitMobile.ExpressionEvaluator.Expressions
{
    abstract class ExpressionQueue<T> : IExpression<T>
    {
        public const string EMPTY = "";

        protected ExpressionFactory _factory;

        protected ExpressionItem<T> _first;
        protected List<ExpressionItem<T>> _expressions;

        public ExpressionQueue(ExpressionFactory factory)
        {
            _factory = factory;
            _expressions = new List<ExpressionItem<T>>();
        }

        public abstract void Handle(string expression);

        public abstract bool IsExpressionEdge(string str, int i);

        public abstract T Evaluate(object root);

        protected void Add(ExpressionItem<T> unit)
        {
            if (_first == null)
                _first = unit;
            else
                _expressions.Add(unit);
        }
    }
}
