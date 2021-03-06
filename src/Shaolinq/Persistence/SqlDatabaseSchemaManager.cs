﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Linq.Expressions;
using System.Transactions;
using Shaolinq.Logging;
using Shaolinq.Persistence.Linq;

namespace Shaolinq.Persistence
{
	public abstract class SqlDatabaseSchemaManager
		: IDisposable
	{
		protected static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

		public SqlDatabaseContext SqlDatabaseContext { get; }
		public ServerSqlDataDefinitionExpressionBuilder ServerSqlDataDefinitionExpressionBuilder { get; }
		
		protected SqlDatabaseSchemaManager(SqlDatabaseContext sqlDatabaseContext)
		{
			this.SqlDatabaseContext = sqlDatabaseContext;
			this.ServerSqlDataDefinitionExpressionBuilder = new ServerSqlDataDefinitionExpressionBuilder(this);
		}

		public virtual void CreateDatabaseAndSchema(DatabaseCreationOptions options)
		{
			var dataDefinitionExpressions = this.BuildDataDefinitonExpressions(options);
			
			if (!this.CreateDatabaseOnly(dataDefinitionExpressions, options))
			{
				return;
			}

			this.CreateDatabaseSchema(dataDefinitionExpressions, options);
		}

		protected virtual SqlDataDefinitionBuilderFlags GetBuilderFlags()
		{
			return SqlDataDefinitionBuilderFlags.BuildTables | SqlDataDefinitionBuilderFlags.BuildIndexes;
		}

		protected abstract bool CreateDatabaseOnly(Expression dataDefinitionExpressions, DatabaseCreationOptions options);

		protected virtual Expression BuildDataDefinitonExpressions(DatabaseCreationOptions options)
		{
			return SqlDataDefinitionExpressionBuilder.Build(this.SqlDatabaseContext.SqlDataTypeProvider, this.SqlDatabaseContext.SqlDialect, this.SqlDatabaseContext.DataAccessModel, options, this.SqlDatabaseContext.TableNamePrefix, this.GetBuilderFlags());
		}

		protected virtual void CreateDatabaseSchema(Expression dataDefinitionExpressions, DatabaseCreationOptions options)
		{
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				using (var dataTransactionContext = this.SqlDatabaseContext.CreateSqlTransactionalCommandsContext(null))
				{
					using (this.SqlDatabaseContext.AcquireDisabledForeignKeyCheckContext(dataTransactionContext))
					{
						var result = this.SqlDatabaseContext.SqlQueryFormatterManager.Format(dataDefinitionExpressions);

						using (var command = dataTransactionContext.CreateCommand(SqlCreateCommandOptions.Default | SqlCreateCommandOptions.UnpreparedExecute))
						{
							command.CommandText = result.CommandText;

							Logger.Debug(command.CommandText);

							command.ExecuteNonQuery();
						}
					}
				}

				scope.Complete();
			}
		}

		public virtual void Dispose()
		{
		}
	}
}
