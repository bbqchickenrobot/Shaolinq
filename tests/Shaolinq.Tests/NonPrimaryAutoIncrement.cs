﻿using System;
using Platform.Validation;

namespace Shaolinq.Tests
{
	[DataAccessObject]
	public abstract class NonPrimaryAutoIncrement
		: DataAccessObject<Guid>
	{
		[Unique]
		[AutoIncrement]
		[PersistedMember]
		public abstract long SerialNumber { get; set; }

		[AutoIncrement]
		[PersistedMember]
		public abstract Guid RandomGuid { get; set; }

	}
}
