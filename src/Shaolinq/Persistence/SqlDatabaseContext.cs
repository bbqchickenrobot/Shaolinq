﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using Shaolinq.Persistence.Linq;

namespace Shaolinq.Persistence
{
	public abstract class SqlDatabaseContext
		: IDisposable
	{
		public TimeSpan? CommandTimeout { get; protected set; }

		protected DbProviderFactory dbProviderFactory;
		internal volatile Dictionary<ProjectorCacheKey, ProjectorCacheInfo> projectorCache = new Dictionary<ProjectorCacheKey, ProjectorCacheInfo>(ProjectorCacheEqualityComparer.Default);
		internal volatile Dictionary<DefaultSqlTransactionalCommandsContext.SqlCommandKey, DefaultSqlTransactionalCommandsContext.SqlCommandValue> formattedInsertSqlCache = new Dictionary<DefaultSqlTransactionalCommandsContext.SqlCommandKey, DefaultSqlTransactionalCommandsContext.SqlCommandValue>(DefaultSqlTransactionalCommandsContext.CommandKeyComparer.Default);
		internal volatile Dictionary<DefaultSqlTransactionalCommandsContext.SqlCommandKey, DefaultSqlTransactionalCommandsContext.SqlCommandValue> formattedUpdateSqlCache = new Dictionary<DefaultSqlTransactionalCommandsContext.SqlCommandKey, DefaultSqlTransactionalCommandsContext.SqlCommandValue>(DefaultSqlTransactionalCommandsContext.CommandKeyComparer.Default);

		public string DatabaseName { get; }
		public string SchemaName { get; protected set; }
		public string[] ContextCategories { get; protected set; }
		public string TableNamePrefix { get; protected set; }
		public DataAccessModel DataAccessModel { get; }
		public SqlDialect SqlDialect { get; protected set; }
		public SqlDataTypeProvider SqlDataTypeProvider { get; protected set; }
		public SqlDatabaseSchemaManager SchemaManager { get; protected set; }
		public SqlQueryFormatterManager SqlQueryFormatterManager { get; protected set; }
		public string ConnectionString { get; protected set; }
		public string ServerConnectionString { get; protected set; }
		
		public abstract DbProviderFactory CreateDbProviderFactory();
		public abstract IDisabledForeignKeyCheckContext AcquireDisabledForeignKeyCheckContext(SqlTransactionalCommandsContext sqlDatabaseCommandsContext);

		public virtual SqlTransactionalCommandsContext CreateSqlTransactionalCommandsContext(Transaction transaction)
		{
			return new DefaultSqlTransactionalCommandsContext(this, transaction);
		}

		public virtual IDbConnection OpenConnection()
		{
			if (this.dbProviderFactory == null)
			{
				this.dbProviderFactory = this.CreateDbProviderFactory();
			}

			var retval = this.dbProviderFactory.CreateConnection();

			retval.ConnectionString = this.ConnectionString;
			retval.Open();

			return retval;
		}

		public virtual IDbConnection OpenServerConnection()
		{
			if (this.dbProviderFactory == null)
			{
				this.dbProviderFactory = this.CreateDbProviderFactory();
			}

			var retval = this.dbProviderFactory.CreateConnection();

			retval.ConnectionString = this.ServerConnectionString;
			retval.Open();

			return retval;
		}

		protected SqlDatabaseContext(DataAccessModel model, SqlDialect sqlDialect, SqlDataTypeProvider sqlDataTypeProvider, SqlQueryFormatterManager sqlQueryFormatterManager, string databaseName, SqlDatabaseContextInfo contextInfo)
		{
			this.DatabaseName = databaseName;
			this.DataAccessModel = model; 
			this.CommandTimeout = contextInfo.CommandTimeout == null ? null : (TimeSpan?)TimeSpan.FromSeconds(contextInfo.CommandTimeout.Value);
			var categories = contextInfo.Categories ?? "";
			this.ContextCategories = categories.Trim().Length == 0 ? new string[0] : categories.Split(',').Select(c => c.Trim()).ToArray();
			this.SqlDialect = sqlDialect;
			this.SqlDataTypeProvider = sqlDataTypeProvider;
			this.SqlQueryFormatterManager = sqlQueryFormatterManager;
			this.SchemaName = EnvironmentSubstitutor.Substitute(contextInfo.SchemaName);
			this.TableNamePrefix = EnvironmentSubstitutor.Substitute(contextInfo.TableNamePrefix);
		}

		public virtual ISqlQueryProvider CreateQueryProvider()
		{
			return new SqlQueryProvider(this.DataAccessModel, this);
		}
		
		public virtual string GetRelatedSql(Exception e)
		{
			return null;
		}

		public virtual Exception DecorateException(Exception exception, DataAccessObject dataAccessObject, string relatedQuery)
		{
			return exception;
		}

		public virtual void DropAllConnections()
		{
		}

		public virtual void Dispose()
		{
			this.SchemaManager.Dispose();
		}
	}
}
