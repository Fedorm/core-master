using System;
using System.Collections.Generic;
using System.Text;
using Mono.Data.Sqlite;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoTouch;

namespace BitMobile.DbEngine
{
	static partial class DbFunctions
	{
		// Because we must set delegate as parameter to MonoPInvokeCallbackAttribute
		public delegate void FakeSQLiteCallback (IntPtr context, int nArgs, IntPtr argptr);

		static object _sqlite3;
		static MethodInfo _sqlite3_GetParamValueType;
		static MethodInfo _sqlite3_GetParamValueInt64;
		static MethodInfo _sqlite3_GetParamValueDouble;
		static MethodInfo _sqlite3_GetParamValueText;
		static MethodInfo _sqlite3_GetParamValueBytes;
		static MethodInfo _sqlite3_ToDateTime;
		static MethodInfo _sqlite3_ReturnNull;
		static MethodInfo _sqlite3_ReturnError;
		static MethodInfo _sqlite3_ReturnInt64;
		static MethodInfo _sqlite3_ReturnDouble;
		static MethodInfo _sqlite3_ReturnText;
		static MethodInfo _sqlite3_ReturnBlob;
		static MethodInfo _sqlite3_ToString;
		static MethodInfo _sqliteConvert_TypeToAffinity;

		public static void Init (SqliteConnection connection)
		{
			FieldInfo connection_sql = connection.GetType ().GetField ("_sql", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3 = connection_sql.GetValue (connection);

			Type sqlite3 = _sqlite3.GetType ();
			_sqlite3_GetParamValueType = sqlite3.GetMethod ("GetParamValueType", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_GetParamValueInt64 = sqlite3.GetMethod ("GetParamValueInt64", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_GetParamValueDouble = sqlite3.GetMethod ("GetParamValueDouble", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_GetParamValueText = sqlite3.GetMethod ("GetParamValueText", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_GetParamValueBytes = sqlite3.GetMethod ("GetParamValueBytes", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ToDateTime = sqlite3.BaseType.GetMethod ("ToDateTime", new Type[] { typeof(string) });

			_sqlite3_ReturnNull = sqlite3.GetMethod ("ReturnNull", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ReturnError = sqlite3.GetMethod ("ReturnError", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ReturnInt64 = sqlite3.GetMethod ("ReturnInt64", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ReturnDouble = sqlite3.GetMethod ("ReturnDouble", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ReturnText = sqlite3.GetMethod ("ReturnText", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ReturnBlob = sqlite3.GetMethod ("ReturnBlob", BindingFlags.Instance | BindingFlags.NonPublic);
			_sqlite3_ToString = sqlite3.GetMethod ("ToString", new Type[] { typeof(DateTime) });
			_sqliteConvert_TypeToAffinity = typeof(SqliteConvert).GetMethod ("TypeToAffinity", BindingFlags.Static | BindingFlags.NonPublic);

			Type SQLiteCallbackDelegate = connection.GetType ().Assembly.GetType ("Mono.Data.Sqlite.SQLiteCallback");
			CreateFunctions (SQLiteCallbackDelegate);
		}

		static void CreateFunctions (Type SQLiteCallbackDelegate)
		{
			MethodInfo CreateFunction = _sqlite3.GetType ().GetMethod ("CreateFunction", BindingFlags.Instance | BindingFlags.NonPublic);

			CreateFunction.Invoke (_sqlite3, new object[] {
				"CONTAINS",
				2,
				false,
				Delegate.CreateDelegate (SQLiteCallbackDelegate, typeof(DbFunctions).GetMethod ("ContainsCallback", BindingFlags.Static | BindingFlags.NonPublic)),
				null,
				null
			});

			CreateFunction.Invoke (_sqlite3, new object[] {
				"TOLOWER",
				1,
				false,
				Delegate.CreateDelegate (SQLiteCallbackDelegate, typeof(DbFunctions).GetMethod ("ToLowerCallback", BindingFlags.Static | BindingFlags.NonPublic)),
				null,
				null
			});

			CreateFunction.Invoke (_sqlite3, new object[] {
				"TOUPPER",
				1,
				false,
				Delegate.CreateDelegate (SQLiteCallbackDelegate, typeof(DbFunctions).GetMethod ("ToUpperCallback", BindingFlags.Static | BindingFlags.NonPublic)),
				null,
				null
			});
		}

		[MonoPInvokeCallback (typeof(FakeSQLiteCallback))]
		static void ToLowerCallback (IntPtr context, int nArgs, IntPtr argptr)
		{
			object[] parms = PrepareParameters (nArgs, argptr);

			object result = ToLower (parms [0].ToString ());

			ReturnValue (context, result);
		}

		[MonoPInvokeCallback (typeof(FakeSQLiteCallback))]
		static void ToUpperCallback (IntPtr context, int nArgs, IntPtr argptr)
		{
			object[] parms = PrepareParameters (nArgs, argptr);

			object result = ToUpper (parms [0].ToString ());

			ReturnValue (context, result);
		}

		[MonoPInvokeCallback (typeof(FakeSQLiteCallback))]
		static void ContainsCallback (IntPtr context, int nArgs, IntPtr argptr)
		{
			object[] parms = PrepareParameters (nArgs, argptr);

			String input = parms [0].ToString ();
			String value = parms [1].ToString ();

			object result = Contains (input, value);

			ReturnValue (context, result);
		}

		static	object[] PrepareParameters (int nArgs, IntPtr argptr)
		{
			object[] parms = new object[nArgs];
			int[] argint = new int[nArgs];
			Marshal.Copy (argptr, argint, 0, nArgs);
			for (int n = 0; n < nArgs; n++) {
				TypeAffinity affinity = (TypeAffinity)_sqlite3_GetParamValueType.InvokeSqlite ((IntPtr)argint [n]);
				switch (affinity) {
				case TypeAffinity.Null:
					parms [n] = DBNull.Value;
					break;
				case TypeAffinity.Int64:
					parms [n] = _sqlite3_GetParamValueInt64.InvokeSqlite ((IntPtr)argint [n]);
					break;
				case TypeAffinity.Double:
					parms [n] = _sqlite3_GetParamValueDouble.InvokeSqlite ((IntPtr)argint [n]);
					break;
				case TypeAffinity.Text:
					parms [n] = _sqlite3_GetParamValueText.InvokeSqlite ((IntPtr)argint [n]);
					break;
				case TypeAffinity.Blob:
					int x;
					byte[] blob;
					x = (int)_sqlite3_GetParamValueBytes.InvokeSqlite ((IntPtr)argint [n], 0, null, 0, 0);
					blob = new byte[x];
					_sqlite3_GetParamValueBytes.InvokeSqlite ((IntPtr)argint [n], 0, blob, 0, 0);
					parms [n] = blob;
					break;
				case TypeAffinity.DateTime:
					object text = _sqlite3_GetParamValueText.InvokeSqlite ((IntPtr)argint [n]);
					parms [n] = _sqlite3_ToDateTime.InvokeSqlite (text);
					break;
				}
			}
			return parms;
		}

		static void ReturnValue (IntPtr context, object result)
		{
			if (result == null || result == DBNull.Value) {
				_sqlite3_ReturnNull.Invoke (_sqlite3, new object[] { context });
				return;
			}

			Type t = result.GetType ();
			if (t == typeof(DateTime)) {
				object str = _sqlite3_ToString.InvokeSqlite (result);
				_sqlite3_ReturnText.InvokeSqlite (context, str);
				return;
			} else {
				Exception r = result as Exception;
				if (r != null) {
					_sqlite3_ReturnError.InvokeSqlite (context, r.Message);
					return;
				}
			}
			TypeAffinity resultAffinity = (TypeAffinity)_sqliteConvert_TypeToAffinity.InvokeSqlite (t);
			switch (resultAffinity) {
			case TypeAffinity.Null:
				_sqlite3_ReturnNull.InvokeSqlite (context);
				return;
			case TypeAffinity.Int64:
				_sqlite3_ReturnInt64.InvokeSqlite (context,	Convert.ToInt64 (result));
				return;
			case TypeAffinity.Double:
				_sqlite3_ReturnDouble.InvokeSqlite (context, Convert.ToDouble (result));
				return;
			case TypeAffinity.Text:
				_sqlite3_ReturnText.InvokeSqlite (context, result.ToString ());
				return;
			case TypeAffinity.Blob:
				_sqlite3_ReturnBlob.InvokeSqlite (context, (byte[])result);
				return;
			}
		}

		static object InvokeSqlite (this MethodInfo mi, params object[] parameters)
		{
			return mi.Invoke (_sqlite3, parameters);
		}
	}
}
