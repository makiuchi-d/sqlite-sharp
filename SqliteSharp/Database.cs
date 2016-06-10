using System;
using System.Collections;

namespace SqliteSharp
{
	public class Database
	{
		string filePath;
		IntPtr db;

		bool isOpen { get { return db != IntPtr.Zero; } }

		public string LastErrorMsg {
			get{
				return Sqlite3.Errmsg(db);
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
				Sqlite3.Close(db);
				db = IntPtr.Zero;
			}
		}

		public Database Open()
		{
			if(isOpen){
				return this;
			}
			db = Sqlite3.Open(filePath);
			return this;
		}

		public Statement Prepare(string query)
		{
			Open();
			IntPtr stmt = Sqlite3.Prepare(db, query);
			return new Statement(db, stmt);
		}

		public Statement Query(string query)
		{
			var stmt = Prepare(query);
			stmt.Execute();
			return stmt;
		}

		public Statement Query(string query, IList param)
		{
			var stmt = Prepare(query);
			stmt.Execute(param);
			return stmt;
		}

		public Statement Query(string query, IDictionary param)
		{
			var stmt = Prepare(query);
			stmt.Execute(param);
			return stmt;
		}

	}
}
