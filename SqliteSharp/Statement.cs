using System;
using System.Collections;
using System.Collections.Generic;

namespace SqliteSharp
{
	public class Statement
	{
		class EnumerableRows: IEnumerable<DataRow>
		{
			readonly IntPtr db;
			readonly IntPtr pStmt;

			public EnumerableRows(IntPtr db, IntPtr pStmt)
			{
				this.db = db;
				this.pStmt = pStmt;
			}

			public IEnumerator<DataRow> GetEnumerator()
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

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		static readonly IEnumerable<DataRow> EmptyRows = new DataRow[0];

		readonly IntPtr db;
		readonly IntPtr pStmt;
		public IEnumerable<DataRow> Rows { get; private set; }

		public Statement(IntPtr db, IntPtr pStmt)
		{
			this.db = db;
			this.pStmt = pStmt;
			Rows = EmptyRows;
		}

		~Statement()
		{
			Sqlite3.Finalize(pStmt);
		}

		public void Bind(int index, object value)
		{
			Sqlite3.Bind(db, pStmt, index, value);
		}

		public void Bind(string name, object value)
		{
			int index = Sqlite3.BindParameterIndex(pStmt, name);
			Bind(index, value);
		}

		public void BindParams(IList param)
		{
			Sqlite3.ClearBindings(db, pStmt);
			var count = param.Count;
			for(var i=0; i<count; ++i){
				Bind(i+1, param[i]);
			}
		}

		public void BindParams(IDictionary param)
		{
			Sqlite3.ClearBindings(db, pStmt);
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
		}

		private bool executeFirstStep()
		{
			bool hasRow = Sqlite3.Step(db, pStmt);
			Rows = (hasRow)? new EnumerableRows(db, pStmt): EmptyRows;
			return hasRow;
		}

		public bool Execute()
		{
			Sqlite3.Reset(db, pStmt);
			return executeFirstStep();
		}

		public bool Execute(IList param)
		{
			Sqlite3.Reset(db, pStmt);
			BindParams(param);
			return executeFirstStep();
		}

		public bool Execute(IDictionary param)
		{
			Sqlite3.Reset(db, pStmt);
			BindParams(param);
			return executeFirstStep();
		}

	}
}
