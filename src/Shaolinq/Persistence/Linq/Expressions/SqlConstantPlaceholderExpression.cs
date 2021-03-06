// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq.Expressions
{
	public class SqlConstantPlaceholderExpression
		: SqlBaseExpression
	{
		public int Index { get; }
		public ConstantExpression ConstantExpression { get; }
		public override ExpressionType NodeType => (ExpressionType)SqlExpressionType.ConstantPlaceholder;

		public SqlConstantPlaceholderExpression(int index, ConstantExpression constantExpression)
			: base(constantExpression.Type)
		{
			this.Index = index;
			this.ConstantExpression = constantExpression;
		}
	}

	public class SqlConstantPlaceholderComparer
		: IEqualityComparer<SqlConstantPlaceholderExpression>
	{
		public static readonly SqlConstantPlaceholderComparer Default = new SqlConstantPlaceholderComparer();

		private SqlConstantPlaceholderComparer()
		{
		}

		public bool Equals(SqlConstantPlaceholderExpression x, SqlConstantPlaceholderExpression y)
		{
			return x.Index == y.Index && x.Type == y.Type;
		}

		public int GetHashCode(SqlConstantPlaceholderExpression obj)
		{
			return obj.Index;
		}
	}
}
