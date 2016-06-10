using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;


namespace SqliteSharp
{
	[TestFixture]
	public class StatementTest
	{
		string dbname = "test.db";

		[SetUp]
		public void CreateDb()
		{
			IntPtr stmt;
			IntPtr db = Sqlite3.Open(dbname);

			stmt = Sqlite3.Prepare(db,"create table test (id integer primary key, value string)");
			Sqlite3.Step(db, stmt);

			stmt = Sqlite3.Prepare(db,"insert into test values (1,'a'),(2,'b'),(3,'c'),(4,'d'),(5,'e'),(6,'f')");
			Sqlite3.Step(db, stmt);

			Sqlite3.Close(db);
		}

		[TearDown]
		public void DeleteDb()
		{
			System.IO.File.Delete(dbname);
		}


		[Test]
		public void ExecuteTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select * from test where id=2");

			stmt.Execute();
			Assert.That(stmt.Rows.First()["value"], Is.EqualTo("b"));
		}

		[Test]
		public void ExecuteWithBindTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select * from test where id=?");

			stmt.Execute(new []{1});
			Assert.That(stmt.Rows.First()["value"], Is.EqualTo("a"));

			stmt.Execute(new Dictionary<object,object>(){{1,3}});
			Assert.That(stmt.Rows.First()["value"], Is.EqualTo("c"));
		}

		[Test]
		public void ExecuteWithNameBindTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select * from test where id=:target");

			stmt.Execute(new Dictionary<object,object>(){{":target",4}});
			Assert.That(stmt.Rows.First()["value"], Is.EqualTo("d"));
		}

		[Test]
		public void BindAndExecuteTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select count(*) from test where id between ? and ?");

			stmt.Bind(2, 4);
			stmt.Bind(1, 2);
			stmt.Execute();
			Assert.That(stmt.Rows.First()["count(*)"], Is.EqualTo(3));
		}

		[Test]
		public void BindArrayAndExecuteTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select count(*) from test where id between ? and ?");

			stmt.BindParams(new []{2,4});
			stmt.Execute();
			Assert.That(stmt.Rows.First()["count(*)"], Is.EqualTo(3));

			stmt.BindParams(new Dictionary<int,int>{{2,6},{1,2}});
			stmt.Execute();
			Assert.That(stmt.Rows.First()["count(*)"], Is.EqualTo(5));
		}

		[Test]
		public void NameBindAndExecuteTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select * from test where id > :target");

			stmt.Bind(":target", 2).Execute();
			Assert.That(stmt.Rows.Count(), Is.EqualTo(4));

			stmt.ClearBindings();
			stmt.Bind(":target", 4).Execute();
			Assert.That(stmt.Rows.Count(), Is.EqualTo(2));
		}

		[Test]
		public void BindDictAndExecuteTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Prepare("select * from test where id > :t1 and id <= :t2");

			stmt.BindParams(new Dictionary<string,object>{{":t1",3},{":t2",5}});
			Assert.That(stmt.Execute().Rows.Count(), Is.EqualTo(2));
		}

		[Test]
		public void SelectEmptyTest()
		{
			var db = new Database(dbname).Open();
			var stmt = db.Query("select * from test where id > 100");
			Assert.That(stmt.Rows.Count(), Is.EqualTo(0));
		}

	}
}

