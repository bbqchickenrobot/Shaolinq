﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Transactions;
using NUnit.Framework;
using Shaolinq.Tests.TestModel;

namespace Shaolinq.Tests
{
	[TestFixture("MySql")]
	[TestFixture("Postgres")]
	[TestFixture("Postgres.DotConnect")]
	[TestFixture("Postgres.DotConnect.Unprepared")]
	[TestFixture("SqlServer", Category = "IgnoreOnMono")]
	[TestFixture("Sqlite")]
	[TestFixture("SqliteInMemory")]
	[TestFixture("SqliteClassicInMemory")]
	public class ConstraintTests
			: BaseTests<TestDataAccessModel>
	{
		public ConstraintTests(string providerName)
			: base(providerName)
		{
		}

		[Test, ExpectedException(typeof(UniqueConstraintException))]
		public void Test_Unique_Non_PrimaryKey_Constraint()
		{
			using (var scope = new TransactionScope())
			{
				var obj1 = this.model.ObjectWithUniqueConstraints.Create();
				obj1.Name = "a";

				var obj2 = this.model.ObjectWithUniqueConstraints.Create();
				obj2.Name = "a";

				scope.Flush(this.model);

				scope.Complete();
			}
		}
	}
}
