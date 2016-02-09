using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Native
{
    public class ClrPropertyDescriptor : Descriptor
    {
        IGlobal global;
        IPropertyGetter getter;

        public ClrPropertyDescriptor(IPropertyGetter getter, IGlobal global, JsDictionaryObject owner, string propertyName)
            : base(owner, propertyName)
        {
            this.global = global;
            this.getter = getter;
        }

        public override JsInstance Get(JsDictionaryObject that)
        {
            object value = getter.GetValue(that.Value, Name).GetValue(that.Value, null);
            return global.Visitor.Return(global.WrapClr(value));
        }

        public override void Set(JsDictionaryObject that, JsInstance value)
        {
            object[] nativeValue = JsClr.ConvertParameters(value);
			System.Reflection.PropertyInfo pi = getter.GetValue (that.Value, Name, nativeValue);

			object val = nativeValue [0];
			if(!pi.PropertyType.Equals(val.GetType()))
			{
				Type t = pi.PropertyType;
				if (t.IsGenericType && t.GetGenericTypeDefinition () == typeof(Nullable<>))
					t = Nullable.GetUnderlyingType (t);
				val = Convert.ChangeType (val, t);
			}

			pi.SetValue(that.Value, val, null);
        }

        internal override DescriptorType DescriptorType
        {
            get { return DescriptorType.Clr; }
        }
    }
}
