using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteSharp
{
	static class Sqlite3
	{
		const int SQLITE_OK = 0;
		const int SQLITE_ROW = 100;
		const int SQLITE_DONE = 101;
		const int SQLITE_INTEGER = 1;
		const int SQLITE_FLOAT = 2;
		const int SQLITE_TEXT = 3;
		const int SQLITE_BLOB = 4;
		const int SQLITE_NULL = 5;

		[DllImport("sqlite3", EntryPoint = "sqlite3_open")]
		static extern int sqlite3_open(string filename, out IntPtr ppDb);

		public static IntPtr Open(string filename)
		{
			IntPtr db;
			if(sqlite3_open(filename, out db) != SQLITE_OK){
				throw new Exception("Could not open database file: " + filename);
			}
			return db;
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_close")]
		static extern int sqlite3_close(IntPtr db);

		public static int Close(IntPtr db)
		{
			return sqlite3_close(db);
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_errmsg")]
		static extern IntPtr sqlite3_errmsg(IntPtr db);

		public static string Errmsg(IntPtr db)
		{
			IntPtr err = sqlite3_errmsg(db);
			return Marshal.PtrToStringAnsi(err);
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2")]
		static extern int sqlite3_prepare_v2(IntPtr db, string zSql, int nByte, out IntPtr ppStmt, IntPtr pzTail);

		public static IntPtr PrepareV2(IntPtr db, string sql)
		{
			IntPtr stmt;
			int len = Encoding.UTF8.GetByteCount(sql);
			if(sqlite3_prepare_v2(db, sql, len, out stmt, IntPtr.Zero) != SQLITE_OK){
				throw new Exception(sqlite3_errmsg(db));
			}
			return stmt;
		}

	}
}
