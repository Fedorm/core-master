using System;
using System.Globalization;
using System.Reflection;
using BitMobile.Common.Controls;

namespace BitMobile.Controls
{
    class InputValidator : IInputValidator
    {
        private readonly object _obj;
        private readonly PropertyInfo _propertyInfo;

        public InputValidator(object obj, string propertyType)
        {
            _obj = obj;
            _propertyInfo = obj.GetType().GetProperty(propertyType);
            if (_propertyInfo == null)
                throw new ArgumentException("Cannot find " + propertyType + " in " + obj.GetType());
        }

        public bool IsNumeric { get; set; }

        public void OnChange(string input, string old)
        {
            if (IsNumeric)
            {
                input = input.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                double result;
                if (!string.IsNullOrWhiteSpace(input)
                    && !double.TryParse(input, NumberStyles.Float, new CultureInfo("en-US"), out result)
                    && !double.TryParse(input, NumberStyles.Float, new CultureInfo("ru-RU"), out result)
                    && input.Trim() != "-" && input.Trim() != ".")
                    _propertyInfo.SetValue(_obj, old);
            }
        }
    }
}
