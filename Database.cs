using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace SqliteSharp
{
	public class Database
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
		private static extern int sqlite3_open(string filename, out IntPtr ppDb);

		[DllImport("sqlite3", EntryPoint = "sqlite3_close")]
		private static extern int sqlite3_close(IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2")]
		private static extern int sqlite3_prepare_v2(IntPtr db, string zSql, int nByte, out IntPtr ppStmt, IntPtr pzTail);
 
		[DllImport("sqlite3", EntryPoint = "sqlite3_errmsg")]
		private static extern IntPtr sqlite3_errmsg(IntPtr db);

		string filePath;
		IntPtr db;

		bool isOpen { get { return db != IntPtr.Zero; } }

		public string LastErrorMsg {
			get{
				var err = sqlite3_errmsg(db);
				return Marshal.PtrToStringAnsi(err);
			}
		}

		public Database(string dbFilePath)
		{
			filePath = dbFilePath;
			db = IntPtr.Zero;
		}

		~Database()
		{
			if(isOpen){
				sqlite3_close(db);
				db = IntPtr.Zero;
			}
		}

		void Open()
		{
			if(isOpen){
				return;
			}
			if(sqlite3_open(filePath, out db) != SQLITE_OK){
				throw new Exception("Could not open database file: " + filePath);
			}
		}

		public Statement Prepare(string query)
		{
			Open();

			int len = Encoding.UTF8.GetByteCount(query);
			IntPtr stmt;
			if(sqlite3_prepare_v2(db, query, len, out stmt, IntPtr.Zero) != SQLITE_OK){
				throw new Exception(sqlite3_errmsg(db));
			}

			return new Statement(stmt);
		}

		public Statement Query(string query)
		{
			var stmt = Prepare(query);
			if(!stmt.Execute()){
				throw new Exception(sqlite3_errmsg(db));
			}
			return stmt;
		}

		public Statement Query(string query, IList param)
		{
			var stmt = Prepare(query);
			if(!stmt.Execute(param)){
				throw new Exception(sqlite3_errmsg(db));
			}
			return stmt;
		}

		public Statement Query(string query, IDictionary param)
		{
			var stmt = Prepare(query);
			if(!stmt.Execute(param)){
				throw new Exception(sqlite3_errmsg(db));
			}
			return stmt;
		}

	}
}
