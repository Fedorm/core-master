namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    interface IMember
    {
        object Invoke(object obj, object root);
    }
}
