// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Shaolinq.Persistence.Linq.Expressions;

namespace Shaolinq.Persistence.Linq
{
	internal class Nominator
		: SqlExpressionVisitor
	{
		public readonly HashSet<Expression> candidates;
		protected Func<Expression, bool> canBeColumn;
		private readonly bool includeIntegralRootExpression;
		private Expression rootExpression;
		private bool inProjection;

		internal Nominator(Func<Expression, bool> canBeColumn, bool includeIntegralRootExpression = false)
		{
			this.canBeColumn = canBeColumn;
			this.includeIntegralRootExpression = includeIntegralRootExpression;
			this.candidates = new HashSet<Expression>();
		}

		public static bool CanBeColumn(Expression expression)
		{
			switch (expression.NodeType)
			{
			case (ExpressionType)SqlExpressionType.Column:
			case (ExpressionType)SqlExpressionType.Scalar:
			case (ExpressionType)SqlExpressionType.FunctionCall:
			case (ExpressionType)SqlExpressionType.AggregateSubquery:
			case (ExpressionType)SqlExpressionType.Aggregate:
			case (ExpressionType)SqlExpressionType.Subquery:
				return true;
			default:
				return false;
			}
		}

		public virtual HashSet<Expression> Nominate(Expression expression)
		{
			if (this.includeIntegralRootExpression)
			{
				this.rootExpression = expression;
			}

			this.Visit(expression);

			return this.candidates;
		}

		protected override Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return null;
			}

			if (expression.NodeType != (ExpressionType)SqlExpressionType.Subquery)
			{
				base.Visit(expression);
			}
			
			if (this.canBeColumn(expression)
				|| (expression.Type.IsIntegralType() && expression == rootExpression))
			{
				this.candidates.Add(expression);
			}
			
			return expression;
		}

		protected override Expression VisitProjection(SqlProjectionExpression projection)
		{
			var saveInProjection = this.inProjection;
			this.inProjection = true;
			base.VisitProjection(projection);
			this.inProjection = saveInProjection;
			return projection;
		}

		protected override Expression VisitJoin(SqlJoinExpression join)
		{
			this.Visit(join.Left);
			this.Visit(join.Right);
			
			if (this.inProjection)
			{
				var saveCanBeColumn = this.canBeColumn;

				this.canBeColumn = c => c is SqlColumnExpression;

				this.Visit(join.JoinCondition);

				this.canBeColumn = saveCanBeColumn;
			}

			return join;
		}

		protected override Expression VisitSelect(SqlSelectExpression selectExpression)
		{
			this.VisitSource(selectExpression.From);
			this.VisitColumnDeclarations(selectExpression.Columns);

			if (inProjection)
			{
				var saveCanBeColumn = this.canBeColumn;

				this.canBeColumn = c => c is SqlColumnExpression;

				this.VisitExpressionList(selectExpression.OrderBy);
				this.VisitExpressionList(selectExpression.GroupBy);
				this.Visit(selectExpression.Skip);
				this.Visit(selectExpression.Take);
				this.Visit(selectExpression.Where);

				this.canBeColumn = saveCanBeColumn;
			}

			return selectExpression;
		}
	}
}