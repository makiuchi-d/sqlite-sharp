using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SqliteSharp
{
	[TestFixture]
	public class DataRowTest
	{
		DataRow dr = new DataRow(){
			{"number", 3},
			{"float", 1.1},
			{"str", "test"},
		};

		[TestCase("number", 3)]
		public void AsIntTest(string key, object value)
		{
			Assert.That(dr.AsInt(key), Is.EqualTo(value));
		}

		[TestCase("str", "test")]
		public void AsStringTest(string key, object value)
		{
			Assert.That(dr.AsString(key), Is.EqualTo(value));
		}

		[TestCase("float", 1.1)]
		public void AsDoubleTest(string key, object value)
		{
			Assert.That(dr.AsDouble(key), Is.EqualTo(value));
		}

	}
}
