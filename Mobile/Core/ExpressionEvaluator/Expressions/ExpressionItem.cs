namespace BitMobile.ExpressionEvaluator.Expressions
{
    class ExpressionItem<T> : IExpression<T>
    {
        IExpression<T> _expression;

        public ExpressionItem(IExpression<T> expression, string prefix)
        {
            _expression = expression;
            Prefix = prefix;
        }

        public string Prefix { get; set; }

        public T Evaluate(object root)
        {
            return _expression.Evaluate(root);
        }
    }
}