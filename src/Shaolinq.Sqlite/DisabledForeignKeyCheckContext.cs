// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using Shaolinq.Persistence;

namespace Shaolinq.Sqlite
{
	internal class DisabledForeignKeyCheckContext
		: IDisabledForeignKeyCheckContext
	{
		public DisabledForeignKeyCheckContext(SqlTransactionalCommandsContext context)
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
