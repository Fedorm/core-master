using System;
using System.Reflection;
using System.Globalization;
using BitMobile.Common.Controls;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.Controls
{
    public class DataBinder : IDataBinder
    {
        private readonly object _control;
        private readonly String _controlPropertyName;
        private readonly String _objPropertyName;
        private readonly IEntity _obj;

        public DataBinder(object control, String controlPropertyName, object obj, String objPropertyName)
        {
            if (obj is IDbRef)
                obj = ((IDbRef)obj).GetObject();
            _obj = (IEntity)obj;

            _objPropertyName = objPropertyName;
            _control = control;

            PropertyInfo pi = control.GetType().GetProperty(controlPropertyName);
            _controlPropertyName = GetDataBindAttribute(pi);
            PropertyChanged();
        }

        public void ControlChanged(object value)
        {
            if (_obj.HasProperty(_objPropertyName))
            {
                object currentValue = _obj.GetValue(_objPropertyName);
                if (currentValue != null && currentValue.Equals(value))
                    return;

                Type propertyType = _obj.EntityType.GetPropertyType(_objPropertyName);
                if (value.GetType() != propertyType)
                {
                    Type t = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    try
                    {
                        value = Convert.ChangeType(value, t);
                    }
                    catch (FormatException)
                    {
                        try
                        {
                            value = Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                            value = SpecificConvert(value, currentValue);
                        }
                    }
                    catch (OverflowException)
                    {
                        value = SpecificConvert(value, currentValue);
                    }
                }
                _obj.SetValue(_objPropertyName, value);
            }
        }

        public bool IsNumeric()
        {
            if (_obj.HasProperty(_objPropertyName))
            {
                Type objectPropertyType = _obj.EntityType.GetPropertyType(_objPropertyName);
                switch (Type.GetTypeCode(objectPropertyType))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        public override string ToString()
        {
            if (_obj.HasProperty(_objPropertyName))
            {
                object currentValue = _obj.GetValue(_objPropertyName);
                return currentValue != null ? currentValue.ToString() : "[NULL]";
            }
            return "[Invalid property name]";
        }

        private String GetDataBindAttribute(PropertyInfo pi)
        {
            object[] attr = pi.GetCustomAttributes(typeof(DataBindAttribute), false);
            if (attr.Length == 0)
                throw new Exception("DataBindAttribute is not found for property " + pi.Name);
            return ((DataBindAttribute)attr[0]).FieldName;
        }

        private void PropertyChanged()
        {
            PropertyInfo pi = _control.GetType().GetProperty(_controlPropertyName);
            if (pi == null)
                throw new Exception(String.Format("Property {0} is not found", _controlPropertyName));


            if (_obj.HasProperty(_objPropertyName))
            {
                object value = _obj.GetValue(_objPropertyName);
                if (value != null)
                    if (value.GetType() != pi.PropertyType)
                        value = Convert.ChangeType(value, pi.PropertyType);
                pi.SetValue(_control, value, null);
            }
        }

        private static object SpecificConvert(object value, object currentValue)
        {
            object result;
            Type currentType = currentValue != null ? currentValue.GetType() : typeof(Object);
            var str = value as string;
            if (str != null)
            {
                if (string.IsNullOrWhiteSpace(str) || str.Trim() == "-" || str.Trim() == "." || str.Trim() == ",")
                {
                    result = currentType.IsValueType ? Activator.CreateInstance(currentType) : null;
                }
                else
                    result = currentValue;
            }
            else
                result = currentValue;

            return result;
        }

    }
}