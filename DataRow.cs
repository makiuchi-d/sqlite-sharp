using System.Collections.Generic;

namespace Sqlite
{
	public class DataRow : Dictionary<string, object>
	{
		public new object this[string column]
		{
			get{
				return (ContainsKey(column))? base[column]: null;
			}
			set{
				if(ContainsKey(column)){
					base[column] = value;
				}
				else{
					Add(column, value);
				}
			}
		}

		public int AsInt(string column){
			return (int)base[column];
		}

		public string AsString(string column){
			return (string)base[column];
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
  
