﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using Shaolinq.Persistence.Linq.Expressions;

namespace Shaolinq.Persistence.Linq
{
	internal class ProjectorCacheEqualityComparer
		: IEqualityComparer<ProjectorCacheKey>
	{
		public static ProjectorCacheEqualityComparer Default = new ProjectorCacheEqualityComparer();

		public bool Equals(ProjectorCacheKey x, ProjectorCacheKey y)
		{
			return x.commandText == y.commandText
				   && SqlExpressionComparer.Equals(x.projectionExpression, y.projectionExpression, SqlExpressionComparerOptions.IgnoreConstantPlaceholders);
		}

		public int GetHashCode(ProjectorCacheKey obj)
		{
			return obj.hashCode;
		}
	}
}