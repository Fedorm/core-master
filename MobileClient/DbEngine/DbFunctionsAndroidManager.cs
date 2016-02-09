using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.DbEngine
{
    public static partial class DbFunctions
    {
        internal static void Init(Mono.Data.Sqlite.SqliteConnection sqliteConnection)
        {
            
            ToLowerFunction.RegisterFunction(typeof(ToLowerFunction));
            ToUpperFunction.RegisterFunction(typeof(ToUpperFunction));
            ContainsFunction.RegisterFunction(typeof(ContainsFunction));
            FormatNumberFunction.RegisterFunction(typeof(FormatNumberFunction));
            FormatDateFunction.RegisterFunction(typeof(FormatDateFunction));

        }

        [SqliteFunction(Name = "TOLOWER", Arguments = 1, FuncType = FunctionType.Scalar)]
        public class ToLowerFunction : Mono.Data.Sqlite.SqliteFunction
        {
            public override object Invoke(object[] args)
            {
                return DbFunctions.ToLower(args[0].ToString());
            }
        }

        [SqliteFunction(Name = "TOUPPER", Arguments = 1, FuncType = FunctionType.Scalar)]
        public class ToUpperFunction : Mono.Data.Sqlite.SqliteFunction
        {
            public override object Invoke(object[] args)
            {
                return DbFunctions.ToUpper(args[0].ToString());
            }
        }

        [SqliteFunction(Name = "CONTAINS", Arguments = 2, FuncType = FunctionType.Scalar)]
        public class ContainsFunction : Mono.Data.Sqlite.SqliteFunction
        {
            public override object Invoke(object[] args)
            {
                String input = args[0].ToString();
                String value = args[1].ToString();

                return DbFunctions.Contains(input, value);
            }
        }

        [SqliteFunction(Name = "FORMATNUMBER", Arguments = 2, FuncType = FunctionType.Scalar)]
        public class FormatNumberFunction : Mono.Data.Sqlite.SqliteFunction
        {
            public override object Invoke(object[] args)
            {
                String format = args[0].ToString();
                String value = args[1].ToString();

                Double v;
                if (Double.TryParse(value, out v))
                    return DbFunctions.Format(format, v);
                else
                    return value;
            }
        }

        [SqliteFunction(Name = "FORMATDATE", Arguments = 2, FuncType = FunctionType.Scalar)]
        public class FormatDateFunction : Mono.Data.Sqlite.SqliteFunction
        {
            public override object Invoke(object[] args)
            {
                String format = args[0].ToString();
                String value = args[1].ToString();

                DateTime v;
                if (DateTime.TryParse(value, out v))
                    return DbFunctions.Format(format, v);
                else
                    return value;
            }
        }


    }
}