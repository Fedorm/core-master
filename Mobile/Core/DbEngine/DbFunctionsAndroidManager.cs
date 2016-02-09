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
    }
}