// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

﻿using System.Collections.Generic;

namespace Shaolinq.Persistence
{
	public class DatabaseMigrationPlan
	{
		public List<MigrationTypeInfo> NewTypes
		{
			get;
			set;
		}

		public List<MigrationTypeInfo> DeletedTypes
		{
			get;
			set;
		}

		public List<MigrationTypeInfo> ModifiedTypes
		{
			get;
			set;
		}

		public DatabaseMigrationPlan()
		{
			this.NewTypes = new List<MigrationTypeInfo>();
			this.ModifiedTypes = new List<MigrationTypeInfo>();
			this.DeletedTypes = new List<MigrationTypeInfo>();
		}
	}
}