using System;
using System.Collections;
using System.Collections.Generic;

namespace SqliteSharp
{
	public class Statement
	{

		static readonly IEnumerable<DataRow> EmptyRows = new DataRow[0];

		readonly IntPtr db;
		readonly IntPtr pStmt;
		public IEnumerable<DataRow> Rows { get; private set; }
		private bool executed;

		public Statement(IntPtr db, IntPtr pStmt)
		{
			this.db = db;
			this.pStmt = pStmt;
			Rows = EmptyRows;
			executed = false;
		}

		~Statement()
		{
			Sqlite3.Finalize(pStmt);
		}

		private void resetIfExecuted()
		{
			if(executed){
				Sqlite3.Reset(db, pStmt);
				executed = false;
			}
		}

		private IEnumerable<DataRow> EnumerableRows()
		{
			var cnames = Sqlite3.ColumnNames(pStmt);
			int cc = cnames.Count;

			do{
				var row = new DataRow();
				for(var i=0; i<cc; ++i){
					row[cnames[i]] = Sqlite3.Column(pStmt, i);
				}
				yield return row;
			}while(Sqlite3.Step(db, pStmt));
		}

		public Statement ClearBindings()
		{
			resetIfExecuted();
			Sqlite3.ClearBindings(db, pStmt);
			return this;
		}

		public Statement Bind(int index, object value)
		{
			Sqlite3.Bind(db, pStmt, index, value);
			return this;
		}

		public Statement Bind(string name, object value)
		{
			int index = Sqlite3.BindParameterIndex(pStmt, name);
			Bind(index, value);
			return this;
		}

		public Statement BindParams(IList param)
		{
			ClearBindings();
			var count = param.Count;
			for(var i=0; i<count; ++i){
				Bind(i+1, param[i]);
			}
			return this;
		}

		public Statement BindParams(IDictionary param)
		{
			ClearBindings();
			foreach(DictionaryEntry kv in param){
				var key = kv.Key;
				if(key is string){
					Bind((string)key, kv.Value);
				}
				else if(key is int){
					Bind((int)key, kv.Value);
				}
				else{
					throw new Exception("invalid binding parameter name type: "+key+", "+key.GetType());
				}
			}
			return this;
		}

		public Statement Execute()
		{
			resetIfExecuted();
			bool hasRow = Sqlite3.Step(db, pStmt);
			Rows = (hasRow)? EnumerableRows(): EmptyRows;
			executed = true;
			return this;
		}

		public Statement Execute(IList param)
		{
			resetIfExecuted();
			BindParams(param);
			return Execute();
		}

		public Statement Execute(IDictionary param)
		{
			resetIfExecuted();
			BindParams(param);
			return Execute();
		}

	}
}
