﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq.Expressions
{
	public class SqlOverExpression
		: SqlBaseExpression
	{
		public Expression Source { get; }
		public IReadOnlyList<Expression> OrderBy { get; }
		public override ExpressionType NodeType => (ExpressionType)SqlExpressionType.Over;
		
		public SqlOverExpression(Expression source, IReadOnlyList<Expression> orderBy)
			: base(typeof(void))
		{
			this.Source = source;
			this.OrderBy = orderBy;
		}
	}
}
