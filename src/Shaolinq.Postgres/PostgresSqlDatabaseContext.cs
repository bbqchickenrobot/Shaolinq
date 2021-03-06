﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using Npgsql;
using Shaolinq.Persistence;

namespace Shaolinq.Postgres
{
	public class PostgresSqlDatabaseContext
		: SqlDatabaseContext
	{
		public int Port { get; }
		public string Host { get; }
		public string UserId { get; }
		public string Password { get; }
		
		public static PostgresSqlDatabaseContext Create(PostgresSqlDatabaseContextInfo contextInfo, DataAccessModel model)
		{
			var constraintDefaults = model.Configuration.ConstraintDefaultsConfiguration;
			var sqlDialect = new PostgresSqlDialect();
			var sqlDataTypeProvider = new PostgresSqlDataTypeProvider(model.TypeDescriptorProvider, constraintDefaults, contextInfo.NativeUuids, contextInfo.NativeEnums);
			var sqlQueryFormatterManager = new DefaultSqlQueryFormatterManager(sqlDialect, sqlDataTypeProvider, (options, sqlDataTypeProviderArg, sqlDialectArg) => new PostgresSqlQueryFormatter(options, sqlDataTypeProviderArg, sqlDialectArg, contextInfo.SchemaName, true));

			return new PostgresSqlDatabaseContext(model, sqlDialect, sqlDataTypeProvider, sqlQueryFormatterManager, contextInfo);
		}

		protected PostgresSqlDatabaseContext(DataAccessModel model, SqlDialect sqlDialect, SqlDataTypeProvider sqlDataTypeProvider, SqlQueryFormatterManager sqlQueryFormatterManager, PostgresSqlDatabaseContextInfo contextInfo)
			: base(model, sqlDialect, sqlDataTypeProvider, sqlQueryFormatterManager, contextInfo.DatabaseName, contextInfo)
		{
			this.Host = contextInfo.ServerName;
			this.UserId = contextInfo.UserId;
			this.Password = contextInfo.Password;
			this.Port = contextInfo.Port;
			
			var connectionStringBuilder = new NpgsqlConnectionStringBuilder
			{
				Host = contextInfo.ServerName,
				Username = contextInfo.UserId,
				Password = contextInfo.Password,
				Port = contextInfo.Port,
				Pooling = contextInfo.Pooling,
				Enlist = false,
				BackendTimeouts = contextInfo.BackendTimeouts,
				MinPoolSize = contextInfo.MinPoolSize,
				MaxPoolSize = contextInfo.MaxPoolSize
			};

			if (contextInfo.ConnectionTimeout.HasValue)
			{
				connectionStringBuilder.Timeout = contextInfo.ConnectionTimeout.Value;
			}
			
			if (contextInfo.ConnectionCommandTimeout.HasValue)
			{
				connectionStringBuilder.CommandTimeout = contextInfo.ConnectionCommandTimeout.Value;
			}

			connectionStringBuilder.Database = contextInfo.DatabaseName;

			this.ConnectionString = connectionStringBuilder.ToString();

			connectionStringBuilder.Database = "postgres";

			this.ServerConnectionString = connectionStringBuilder.ToString();

			this.SchemaManager = new PostgresSqlDatabaseSchemaManager(this);
		}

		public override SqlTransactionalCommandsContext CreateSqlTransactionalCommandsContext(Transaction transaction)
		{
			return new PostgresSqlTransactionalCommandsContext(this, transaction);
		}

		public override DbProviderFactory CreateDbProviderFactory()
		{
			return NpgsqlFactory.Instance;
		}

		public override IDisabledForeignKeyCheckContext AcquireDisabledForeignKeyCheckContext(SqlTransactionalCommandsContext sqlDatabaseCommandsContext)
		{
			return new DisabledForeignKeyCheckContext(sqlDatabaseCommandsContext);	
		}

		public override void DropAllConnections()
		{
			NpgsqlConnection.ClearAllPools();
		}
		
		public override Exception DecorateException(Exception exception, DataAccessObject dataAccessObject, string relatedQuery)
		{
			var postgresException = exception as NpgsqlException;

			if (postgresException == null)
			{
				return base.DecorateException(exception, dataAccessObject, relatedQuery);
			}

			switch (postgresException.Code)
			{
			case "40001":
				return new ConcurrencyException(exception, relatedQuery);
			case "23502":
				return new MissingPropertyValueException(dataAccessObject, postgresException, relatedQuery);
			case "23503":
				return new MissingRelatedDataAccessObjectException(null, dataAccessObject, postgresException, relatedQuery);
			case "23505":
				if (!string.IsNullOrEmpty(postgresException.ColumnName) && dataAccessObject != null)
				{
					if (dataAccessObject.GetAdvanced().GetPrimaryKeysFlattened().Any(c => c.PersistedName == postgresException.ColumnName))
					{
						return new ObjectAlreadyExistsException(dataAccessObject, exception, relatedQuery);
					}
				}

				if (!string.IsNullOrEmpty(postgresException.ConstraintName) && postgresException.ConstraintName.EndsWith("_pkey"))
				{
					return new ObjectAlreadyExistsException(dataAccessObject, exception, relatedQuery);
				}

				if (!string.IsNullOrEmpty(postgresException.Detail) && dataAccessObject != null)
				{
					if (dataAccessObject.GetAdvanced().GetPrimaryKeysFlattened().Any(c => Regex.Match(postgresException.Detail, @"Key\s*\(\s*""?" + c.PersistedName + @"""?\s*\)", RegexOptions.CultureInvariant).Success))
					{
						return new ObjectAlreadyExistsException(dataAccessObject, exception, relatedQuery);	
					}
				}

				if (postgresException.Message.IndexOf("_pkey", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					return new ObjectAlreadyExistsException(dataAccessObject, exception, relatedQuery);	
				}
				else
				{
					return new UniqueConstraintException(exception, relatedQuery);
				}
			}
			
			return new DataAccessException(postgresException, relatedQuery);
		}
	}
}
