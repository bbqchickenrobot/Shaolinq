﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq.Expressions
{
	public class SqlReferencesColumnExpression
		: SqlBaseExpression
	{
		public SqlTableExpression ReferencedTable {get; }
		public SqlColumnReferenceDeferrability Deferrability { get; }
		public SqlColumnReferenceAction OnDeleteAction { get; }
		public SqlColumnReferenceAction OnUpdateAction { get; }
		public IReadOnlyList<string> ReferencedColumnNames { get; }
		public override ExpressionType NodeType => (ExpressionType)SqlExpressionType.ReferencesColumn;

		public SqlReferencesColumnExpression(SqlTableExpression referencedTable, SqlColumnReferenceDeferrability deferrability, IEnumerable<string> referencedColumnNames, SqlColumnReferenceAction onDelete, SqlColumnReferenceAction onUpdate)
			: this(referencedTable, deferrability, referencedColumnNames.ToReadOnlyCollection(), onDelete, onUpdate)
		{
		}

		public SqlReferencesColumnExpression(SqlTableExpression referencedTable, SqlColumnReferenceDeferrability deferrability, IReadOnlyList<string> referencedColumnNames, SqlColumnReferenceAction onDelete, SqlColumnReferenceAction onUpdate)
			: base(typeof(void))
		{
			this.OnDeleteAction = onDelete;
			this.OnUpdateAction = onUpdate;
			this.ReferencedTable = referencedTable;
			this.Deferrability = deferrability;
			this.ReferencedColumnNames = referencedColumnNames;
		}

		public SqlReferencesColumnExpression UpdateReferencedColumnNames(IEnumerable<string> columnNames)
		{
			return new SqlReferencesColumnExpression(this.ReferencedTable, this.Deferrability, columnNames, this.OnDeleteAction, this.OnUpdateAction);
		}
	}
}
