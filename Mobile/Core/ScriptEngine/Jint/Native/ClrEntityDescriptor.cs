namespace Jint.Native
{
    class ClrEntityDescriptor : Descriptor
    {
        private readonly IEntityAccessor _entityAccessor;
        private readonly IGlobal _global;

        public ClrEntityDescriptor(IEntityAccessor entityAccessor, IGlobal global, JsDictionaryObject owner, string name)
            : base(owner, name)
        {
            _entityAccessor = entityAccessor;
            _global = global;
        }

        public override JsInstance Get(JsDictionaryObject that)
        {
            object value = _entityAccessor.GetValue(that.Value, Name);
            return _global.Visitor.Return(_global.WrapClr(value));
        }

        public override void Set(JsDictionaryObject that, JsInstance value)
        {
           _entityAccessor.SetValue(that.Value, Name, value.Value);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Clr; }
        }
    }
}
