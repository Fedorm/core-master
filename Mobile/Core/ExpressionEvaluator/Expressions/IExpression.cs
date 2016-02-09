namespace BitMobile.ExpressionEvaluator.Expressions
{
    interface IExpression<T>
    {
        T Evaluate(object root);
    }    
}
