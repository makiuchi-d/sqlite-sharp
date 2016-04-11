using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteSharp
{
	static class Sqlite3
	{
		const string dllname = "sqlite3";

		public const int SQLITE_OK = 0;
		public const int SQLITE_ROW = 100;
		public const int SQLITE_DONE = 101;

		public const int SQLITE_INTEGER = 1;
		public const int SQLITE_FLOAT = 2;
		public const int SQLITE_TEXT = 3;
		public const int SQLITE_BLOB = 4;
		public const int SQLITE_NULL = 5;

		[DllImport(dllname, EntryPoint = "sqlite3_open")]
		static extern int sqlite3_open(string filename, out IntPtr ppDb);

		public static IntPtr Open(string filename)
		{
			IntPtr db;
			if(sqlite3_open(filename, out db) != SQLITE_OK){
				throw new Exception("Could not open database file: " + filename);
			}
			return db;
		}

		[DllImport(dllname, EntryPoint = "sqlite3_close")]
		static extern int sqlite3_close(IntPtr db);

		public static int Close(IntPtr db)
		{
			return sqlite3_close(db);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_errmsg")]
		static extern IntPtr sqlite3_errmsg(IntPtr db);

		public static string Errmsg(IntPtr db)
		{
			IntPtr err = sqlite3_errmsg(db);
			return Marshal.PtrToStringAnsi(err);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_prepare_v2")]
		static extern int sqlite3_prepare_v2(IntPtr db, string zSql, int nByte, out IntPtr ppStmt, IntPtr pzTail);

		public static IntPtr Prepare(IntPtr db, string sql)
		{
			IntPtr stmt;
			int len = Encoding.UTF8.GetByteCount(sql);
			if(sqlite3_prepare_v2(db, sql, len, out stmt, IntPtr.Zero) != SQLITE_OK){
				throw new Exception(sqlite3_errmsg(db));
			}
			return stmt;
		}

		[DllImport(dllname, EntryPoint = "sqlite3_finalize")]
		static extern int sqlite3_finalize(IntPtr pStmt);

		public static int Finalize(IntPtr pStmt)
		{
			return sqlite3_finalize(pStmt);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_reset")]
		static extern int sqlite3_reset(IntPtr pStmt);

		public static int Reset(IntPtr pStmt)
		{
			return sqlite3_reset(pStmt);
		}

		public static void Reset(IntPtr db, IntPtr pStmt)
		{
			if(Reset(pStmt) != SQLITE_OK){
				throw new Exception(Errmsg(db));
			}
		}

		[DllImport(dllname, EntryPoint = "sqlite3_clear_bindings")]
		static extern int sqlite3_clear_bindings(IntPtr pStmt);

		public static int ClearBindings(IntPtr pStmt)
		{
			return sqlite3_clear_bindings(pStmt);
		}

		public static void ClearBindings(IntPtr db, IntPtr pStmt)
		{
			if(ClearBindings(pStmt) != SQLITE_OK){
				throw new Exception(Errmsg(db));
			}
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_parameter_index")]
		static extern int sqlite3_bind_parameter_index(IntPtr pStmt, string zName);

		public static int BindParameterIndex(IntPtr pStmt, string name)
		{
			return sqlite3_bind_parameter_index(pStmt, name);
		}

		public static void BindParameterIndex(IntPtr db, IntPtr pStmt, string name)
		{
			int index = BindParameterIndex(pStmt, name);
			if(index == 0){
				throw new Exception(Errmsg(db));
			}
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_blob")]
		static extern int sqlite3_bind_blob(IntPtr pStmt, int i, byte[] data, int len , IntPtr func);

		public static int BindBlob(IntPtr pStmt, int i, byte[] value)
		{
			var len = value.Length;
			return sqlite3_bind_blob(pStmt, i, value, len, IntPtr.Zero);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_double")]
		static extern int sqlite3_bind_double(IntPtr pStmt, int i, double value);

		public static int BindDouble(IntPtr pStmt, int i, double value)
		{
			return sqlite3_bind_double(pStmt, i, value);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_int")]
		static extern int sqlite3_bind_int(IntPtr pStmt, int i, int value);

		public static int BindInt(IntPtr pStmt, int i, int value)
		{
			return sqlite3_bind_int(pStmt, i, value);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_null")]
		static extern int sqlite3_bind_null(IntPtr pStmt, int i);

		public static int BindNull(IntPtr pStmt, int i)
		{
			return sqlite3_bind_null(pStmt, i);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_bind_text")]
		static extern int sqlite3_bind_text(IntPtr pStmt, int i, byte[] value, int nByte, IntPtr func);

		public static int BindText(IntPtr pStmt, int i, string value)
		{
			var len = Encoding.UTF8.GetByteCount(value)+1;
			var data = new byte[len];
			len = Encoding.UTF8.GetBytes(value, 0, value.Length, data, 0);
			return sqlite3_bind_text(pStmt, i, data, len, IntPtr.Zero);
		}

		//[DllImport(dllname, EntryPoint = "sqlite3_bind_value")]
		//static extern int sqlite3_bind_value(IntPtr pStmt, int i, IntPtr pValue);

		[DllImport(dllname, EntryPoint = "sqlite3_bind_zeroblob")]
		static extern int sqlite3_bind_zeroblob(IntPtr pStmt, int i, int n);

		public static int BindZeroBlob(IntPtr pStmt, int i, int n)
		{
			return sqlite3_bind_zeroblob(pStmt, i, n);
		}

		public static void Bind(IntPtr db, IntPtr pStmt, int i, object value)
		{
			int result;
			if(value == null){
				result = Sqlite3.BindNull(pStmt, i);
			}
			else if(value is int || value is long || value is short){
				result = BindInt(pStmt, i, (int)value);
			}
			else if(value is string){
				result = BindText(pStmt, i, (string)value);
			}
			else if(value is float || value is double){
				result = BindDouble(pStmt, i, (double)value);
			}
			else if(value is byte[]){
				result = BindBlob(pStmt, i, (byte[])value);
			}
			else{
				throw new Exception("invalid binding parameter type: ("+i+") "+value.GetType());
			}
			if(result != SQLITE_OK){
				throw new Exception(Errmsg(db));
			}
		}

		[DllImport(dllname, EntryPoint = "sqlite3_step")]
		static extern int sqlite3_step(IntPtr pStmt);

		public static bool Step(IntPtr db, IntPtr pStmt)
		{
			int result = sqlite3_step(pStmt);

			if(result != SQLITE_ROW && result != SQLITE_DONE){
				throw new Exception(Errmsg(db));
			}

			return result == SQLITE_ROW;
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_count")]
		static extern int sqlite3_column_count(IntPtr pStmt);

		public static int ColumnCount(IntPtr pStmt)
		{
			return sqlite3_column_count(pStmt);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_name")]
		static extern IntPtr sqlite3_column_name(IntPtr pStmt, int iCol);

		public static string ColumnName(IntPtr pStmt, int iCol)
		{
			var pStr = sqlite3_column_name(pStmt, iCol);
			return Marshal.PtrToStringAnsi(pStr);
		}

		public static List<string> ColumnNames(IntPtr pStmt)
		{
			int cc = ColumnCount(pStmt);
			var names = new List<string>(cc);
			for(var i=0; i<cc; ++i){
				names.Add(ColumnName(pStmt, i));
			}
			return names;
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_type")]
		static extern int sqlite3_column_type(IntPtr pStmt, int iCol);

		public static int ColumnType(IntPtr pStmt, int iCol)
		{
			return sqlite3_column_type(pStmt, iCol);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_int")]
		static extern int sqlite3_column_int(IntPtr pStmt, int iCol);

		public static int ColumnInt(IntPtr pStmt, int iCol)
		{
			return sqlite3_column_int(pStmt, iCol);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_text")]
		static extern IntPtr sqlite3_column_text(IntPtr pStmt, int iCol);

		public static string ColumnText(IntPtr pStmt, int iCol)
		{
			var pStr = sqlite3_column_text(pStmt, iCol);
			return Marshal.PtrToStringAnsi(pStr);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_double")]
		static extern double sqlite3_column_double(IntPtr pStmt, int iCol);

		public static double ColumnDouble(IntPtr pStmt, int iCol)
		{
			return sqlite3_column_double(pStmt, iCol);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_bytes")]
		static extern int sqlite3_column_bytes(IntPtr pStmt, int iCol);

		public static int ColumnBytes(IntPtr pStmt, int iCol)
		{
			return sqlite3_column_bytes(pStmt, iCol);
		}

		[DllImport(dllname, EntryPoint = "sqlite3_column_blob")]
		static extern IntPtr sqlite3_column_blob(IntPtr pStmt, int iCol);

		public static byte[] ColumnBlob(IntPtr pStmt, int iCol)
		{
			int size = ColumnBytes(pStmt, iCol);
			var pBlob = sqlite3_column_blob(pStmt, iCol);

			var data = new byte[size];
			Marshal.Copy(pBlob, data, 0, size);

			return data;
		}

		public static object Column(IntPtr pStmt, int iCol)
		{
			switch(ColumnType(pStmt, iCol)){
				case SQLITE_INTEGER:
					return ColumnInt(pStmt, iCol);

				case SQLITE_FLOAT:
					return ColumnDouble(pStmt, iCol);

				case SQLITE_TEXT:
					return ColumnText(pStmt, iCol);

				case SQLITE_BLOB:
					return ColumnBlob(pStmt, iCol);

				case SQLITE_NULL:
				default:
					return null;
			}
		}
	}
}
