﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Shaolinq.Tests.ComplexPrimaryKeyModel
{
	[DataAccessObject]
	public abstract class Mall
		: DataAccessObject<Guid>
	{
		[PersistedMember]
		public abstract string Name { get; set; }

		[PersistedMember]
		public abstract Address Address { get; set; }

		[RelatedDataAccessObjects]
		public abstract RelatedDataAccessObjects<Shop> Shops { get; }
	}
}
