using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BitMobile.ClientModel
{
    public class Converter
    {
        public double ToDouble(object obj)
        {
            double result;

            if (obj is string)
            {
                string s = obj.ToString();
                if (!double.TryParse(s, out result))
                    result = double.Parse(s, CultureInfo.InvariantCulture);
            }
            else
                try
                {
                    result = Convert.ToDouble(obj);
                }
                catch
                {
                    result = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                }


            return result;
        }

        public decimal ToDecimal(object obj)
        {
            decimal result;

            if (obj is string)
            {
                string s = obj.ToString();
                if (!decimal.TryParse(s, out result))
                    result = decimal.Parse(s, CultureInfo.InvariantCulture);
            }
            else
                try
                {
                    result = Convert.ToDecimal(obj);
                }
                catch
                {
                    result = Convert.ToDecimal(obj, CultureInfo.InvariantCulture);
                }


            return result;
        }
    }
}
