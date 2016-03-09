using System;
using System.Runtime.InteropServices;

namespace SqliteSharp
{
	public class Exception: System.Exception
	{
		public Exception(string message): base(message)
		{
		}
		public Exception(IntPtr errmsgptr): base(Marshal.PtrToStringAnsi(errmsgptr))
		{
		}
	}
}
