﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Shaolinq.TypeBuilding
{
	public abstract class DataAccessAssemblyProvider
	{
		public abstract RuntimeDataAccessModelInfo GetDataAccessModelAssembly(Type dataAccessModelType, DataAccessModelConfiguration configuration);
	}
}