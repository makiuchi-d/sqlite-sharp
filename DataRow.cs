using System.Collections.Generic;

namespace SqliteSharp
{
	public class DataRow : Dictionary<string, object>
	{
		public int AsInt(string column){
			return (int)base[column];
		}

		public string AsString(string column){
			return (string)base[column];
		}

		public double AsDouble(string column){
			return (double)base[column];
		}

		public override string ToString()
		{
			string str = "{";
			foreach(var key in Keys){
				str += "" + key + ":" + this[key] + ",";
			}
			str += "}";
			return str;
		}
	}
}
  
