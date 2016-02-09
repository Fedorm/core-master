using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace Tests
{
    class Dialog : RemoteProxy
    {
        public Dialog(String address, Console console)
            : base(address, console)
        {
        }

        public string ClickPositive()
        {
            return DoRequestString("DialogClickPositive");
        }

        public string ClickNegative()
        {
            return DoRequestString("DialogClickNegative");
        }

        public string GetMessage()
        {
            return DoRequestString("DialogGetMessage");
        }

        public DateTime GetDateTime()
        {
            string response = DoRequestString("DialogGetDateTime");

            DateTime result;
            if (!DateTime.TryParse(response, out result))
                if (!DateTime.TryParse(response, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    throw new Exception(string.Format("Cannot parse '{0}' to DateTime", response));

            return result;
        }        

        public string SetDateTime(string value)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(value, out dateTime))
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    throw new Exception(string.Format("Cannot parse '{0}' to DateTime", value));
            return DoRequestString("DialogSetDateTime", "null", dateTime);
        }

        public string SelectItem(int index)
        {
            return DoRequestString("DialogSelectItem", "null", index);
        }

        public string GetItem(int index)
        {
            return DoRequestString("DialogGetItem", "null", index);
        }
    }
}
