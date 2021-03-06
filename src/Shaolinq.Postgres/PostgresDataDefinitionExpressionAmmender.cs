﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Linq;
using System.Linq.Expressions;
using Shaolinq.Persistence;
using Shaolinq.Persistence.Linq;
using Shaolinq.Persistence.Linq.Expressions;

namespace Shaolinq.Postgres
{
	public class PostgresDataDefinitionExpressionAmender
		: SqlExpressionVisitor
	{
		private readonly SqlDataTypeProvider sqlDataTypeProvider;
		private bool currentIsPrimaryKey;

		private PostgresDataDefinitionExpressionAmender(SqlDataTypeProvider sqlDataTypeProvider)
		{
			this.sqlDataTypeProvider = sqlDataTypeProvider;
		}

		public static Expression Amend(Expression expression, SqlDataTypeProvider sqlDataTypeProvider)
		{
			var processor = new PostgresDataDefinitionExpressionAmender(sqlDataTypeProvider);

			return processor.Visit(expression);
		}
		
		protected override Expression VisitSimpleConstraint(SqlSimpleConstraintExpression simpleConstraintExpression)
		{
			if (this.currentIsPrimaryKey && simpleConstraintExpression.Constraint == SqlSimpleConstraint.AutoIncrement)
			{
				return null;
			}

			return base.VisitSimpleConstraint(simpleConstraintExpression);
		}

		protected override Expression VisitColumnDefinition(SqlColumnDefinitionExpression columnDefinitionExpression)
		{
			this.currentIsPrimaryKey = columnDefinitionExpression.ConstraintExpressions
				.OfType<SqlSimpleConstraintExpression>()
				.Any(c => c.Constraint == SqlSimpleConstraint.PrimaryKey);

			var isAutoIncrement = columnDefinitionExpression.ConstraintExpressions
				.OfType<SqlSimpleConstraintExpression>()
				.Any(c => c.Constraint == SqlSimpleConstraint.AutoIncrement);

			var retval = (SqlColumnDefinitionExpression)base.VisitColumnDefinition(columnDefinitionExpression);

			if (isAutoIncrement)
			{
				var longTypeSqlName = this.sqlDataTypeProvider.GetSqlDataType(typeof(long)).GetSqlName(null);

				if (((SqlTypeExpression)columnDefinitionExpression.ColumnType).TypeName == longTypeSqlName)
				{
					retval = new SqlColumnDefinitionExpression(retval.ColumnName, new SqlTypeExpression("BIGSERIAL"), retval.ConstraintExpressions);
				}
				else
				{
					retval = new SqlColumnDefinitionExpression(retval.ColumnName, new SqlTypeExpression("SERIAL"), retval.ConstraintExpressions);
				}
			}

			return retval;
		}
	}
}
