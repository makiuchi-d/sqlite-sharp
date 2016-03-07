using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace Sqlite
{
	public class Statement
	{
		const int SQLITE_OK = 0;
		const int SQLITE_ROW = 100;
		const int SQLITE_DONE = 101;

		const int SQLITE_INTEGER = 1;
		const int SQLITE_FLOAT = 2;
		const int SQLITE_TEXT = 3;
		const int SQLITE_BLOB = 4;
		const int SQLITE_NULL = 5;


		[DllImport("sqlite3", EntryPoint = "sqlite3_step")]
		private static extern int sqlite3_step(IntPtr pStmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_finalize")]
		private static extern int sqlite3_finalize(IntPtr pStmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_reset")]
		private static extern int sqlite3_reset(IntPtr pStmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_clear_bindings")]
		private static extern int sqlite3_clear_bindings(IntPtr pStmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_parameter_index")]
		private static extern int sqlite3_bind_parameter_index(IntPtr pStmt, string zName);

		//[DllImport("sqlite3", EntryPoint = "sqlite3_bind_blob")]
		//private static extern int sqlite3_bind_blob(IntPtr pStmt, int i, IntPtr data, int len , IntPtr func);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_double")]
		private static extern int sqlite3_bind_double(IntPtr pStmt, int i, double value);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_int")]
		private static extern int sqlite3_bind_int(IntPtr pStmt, int i, int value);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_null")]
		private static extern int sqlite3_bind_null(IntPtr pStmt, int i);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_text")]
		private static extern int sqlite3_bind_text(IntPtr pStmt, int i, byte[] value, int nByte, IntPtr func);

		//[DllImport("sqlite3", EntryPoint = "sqlite3_bind_value")]
		//private static extern int sqlite3_bind_value(sqlite3_stmt*, int, const sqlite3_value*);

		//[DllImport("sqlite3", EntryPoint = "sqlite3_bind_zeroblob")]
		//private static extern int sqlite3_bind_zeroblob(sqlite3_stmt*, int, int n);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_count")]
		private static extern int sqlite3_column_count(IntPtr pStmt);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_name")]
		private static extern IntPtr sqlite3_column_name(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_type")]
		private static extern int sqlite3_column_type(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int")]
		private static extern int sqlite3_column_int(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_text")]
		private static extern IntPtr sqlite3_column_text(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_double")]
		private static extern double sqlite3_column_double(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_blob")]
		private static extern IntPtr sqlite3_column_blob(IntPtr pStmt, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_bytes")]
		private static extern int sqlite3_column_bytes(IntPtr pStmt, int iCol);

		readonly IntPtr pStmt;
		List<string> fetchingColumns = new List<string>();
		bool executed;
		int lastStepResult;

		public Statement(IntPtr pStmt)
		{
			this.pStmt = pStmt;
		}

		~Statement()
		{
			sqlite3_finalize(pStmt);
		}

		public void Bind(int index, object value)
		{
			if(value == null){
				sqlite3_bind_null(pStmt, index);
			}
			else if(value is int){
				sqlite3_bind_int(pStmt, index, (int)value);
			}
			else if(value is string){
				var str = (string)value;
				var len = Encoding.UTF8.GetByteCount(str) +1;
				var data = new byte[len];
				len = Encoding.UTF8.GetBytes(str, 0, str.Length, data, 0);
				sqlite3_bind_text(pStmt, index, data, len, IntPtr.Zero);
			}
			else if(value is float || value is double){
				sqlite3_bind_double(pStmt, index, (double)value);
			}
			else{
				throw new Exception("invalid binding parameter type: ("+index+") "+value.GetType());
			}
		}

		public void Bind(string name, object value)
		{
			int index = sqlite3_bind_parameter_index(pStmt, name);
			Bind(index, value);
		}

		public void BindParams(IList param)
		{
			sqlite3_clear_bindings(pStmt);
			var count = param.Count;
			for(var i=0; i<count; ++i){
				Bind(i+1, param[i]);
			}
		}

		public void BindParams(IDictionary param)
		{
			sqlite3_clear_bindings(pStmt);
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

		void resetIfExecuted()
		{
			if(executed){
				sqlite3_reset(pStmt);
				fetchingColumns.Clear();
			}
			executed = false;
		}

		public bool Execute()
		{
			resetIfExecuted();

			lastStepResult = sqlite3_step(pStmt);
			executed = true;

			return lastStepResult == SQLITE_DONE || lastStepResult == SQLITE_ROW;
		}

		public bool Execute(IList param)
		{
			resetIfExecuted();
			BindParams(param);
			return Execute();
		}

		public bool Execute(IDictionary param)
		{
			resetIfExecuted();
			BindParams(param);
			return Execute();
		}

		List<string> fetchColumnNames(){
			if(fetchingColumns.Count == 0){
				int columnCount = sqlite3_column_count(pStmt);
				for(var i=0; i<columnCount; ++i){
					fetchingColumns.Add(Marshal.PtrToStringAnsi(sqlite3_column_name(pStmt, i)));
				}
			}
			return fetchingColumns;
		}

		object fetchColumn(int iCol)
		{
			switch(sqlite3_column_type(pStmt, iCol)){
			case SQLITE_INTEGER:
				return (object)sqlite3_column_int(pStmt, iCol);
			case SQLITE_FLOAT:
				return (object)sqlite3_column_double(pStmt, iCol);
			case SQLITE_TEXT:
				var pText = sqlite3_column_text(pStmt, iCol);
				return Marshal.PtrToStringAnsi(pText);
			case SQLITE_BLOB:
				IntPtr blob = sqlite3_column_blob(pStmt, iCol);
				int size = sqlite3_column_bytes(pStmt, iCol);
				var data = new byte[size];
				Marshal.Copy(blob, data, 0, size);
				return data;
			}
			return null;
		}

		public DataRow Fetch()
		{
			if(lastStepResult != SQLITE_ROW){
				return null;
			}

			var columns = fetchColumnNames();

			var row = new DataRow();
			for(var i=0; i<columns.Count; ++i){
				row[columns[i]] = fetchColumn(i);
			}

			lastStepResult = sqlite3_step(pStmt);
			return row;
		}

		public List<DataRow> FetchAll()
		{
			var s = new List<DataRow>();
			DataRow row;
			while((row=Fetch())!=null){
				s.Add(row);
			}
			return s;
		}

	}
}
