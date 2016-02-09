using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Schema
{
    // ReSharper disable InconsistentNaming
    public abstract class SchemaCombinator
    {
        public Int32 id { get; set; }
        public List<Param> @params { get; set; }
        public string type { get; set; }

        public abstract string GetName();

        public override string ToString()
        {
            return ToString(true);
        }

        protected virtual string ToString(bool needId)
        {
            var sb = new StringBuilder();

            if (needId)
            {
                sb.Append("#");
                sb.Append(id.ToString("x"));
            }

            foreach (Param item in @params)
            {
                sb.Append(" ");
                sb.Append(item);
            }

            sb.Append(" = ");
            sb.Append(type);
            return sb.ToString();
        }
    }
}
