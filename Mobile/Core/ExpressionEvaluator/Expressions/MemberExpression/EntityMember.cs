using BitMobile.ValueStack;

namespace BitMobile.ExpressionEvaluator.Expressions.MemberExpression
{
    class EntityMember : IMember
    {
        private readonly string _propertyName;

        public EntityMember(string propertyName)
        {
            _propertyName = propertyName;
        }

        public object Invoke(object obj, object root)
        {
            var entity = (IEntity)obj;
            return entity.GetValue(_propertyName);
        }
    }
}