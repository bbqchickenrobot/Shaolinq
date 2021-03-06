// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Shaolinq.Persistence
{
	public abstract class SqlDataType
	{
		protected readonly ConstraintDefaultsConfiguration constraintDefaultsConfiguration;
		protected static readonly MethodInfo IsDbNullMethod = DataRecordMethods.IsNullMethod;

		public Type SupportedType { get; }
		public Type UnderlyingType { get; }
		public bool IsUserDefinedType { get; }

		/// <summary>
		/// Converts the given value for serializing to SQL.  The default
		/// implementation performs no conversion.
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>The converted value</returns>
		public virtual Tuple<Type, object> ConvertForSql(object value)
		{
			if (this.UnderlyingType != null)
			{
				return new Tuple<Type, object>(this.UnderlyingType, value);
			}
			else
			{
				return new Tuple<Type, object>(this.SupportedType, value);
			}
		}

		/// <summary>
		/// Converts a value from SQL to a .NET equivalent.  The default implementation
		/// uses <see cref="Convert.ChangeType(object, Type)"/> and performs <see cref="DBNull"/>
		/// conversion
		/// </summary>
		/// <param name="value">The value to convert</param>
		/// <returns>The converted value</returns>
		public virtual object ConvertFromSql(object value)
		{
			if (this.UnderlyingType != null)
			{
				if (value == null || value == DBNull.Value)
				{
					return null;
				}

				return Convert.ChangeType(value, this.UnderlyingType);
			}
			else
			{
				return Convert.ChangeType(value, this.SupportedType);
			}
		}


		protected SqlDataType(ConstraintDefaultsConfiguration constraintDefaultsConfiguration, Type supportedType)
			: this(constraintDefaultsConfiguration, supportedType, false)
		{	
		}

		protected SqlDataType(ConstraintDefaultsConfiguration constraintDefaultsConfiguration, Type supportedType, bool isUserDefinedType)
		{
			this.constraintDefaultsConfiguration = constraintDefaultsConfiguration;
			this.SupportedType = supportedType;
			this.IsUserDefinedType = isUserDefinedType;
			this.UnderlyingType = Nullable.GetUnderlyingType(supportedType);
		}

		/// <summary>
		/// Gets the SQL type name for the given property.
		/// </summary>
		/// <param name="propertyDescriptor">The property whose return type is to be serialized</param>
		/// <returns>The SQL type name</returns>
		public abstract string GetSqlName(PropertyDescriptor propertyDescriptor);

		/// <summary>
		/// Gets an expression to perform reading of a column.
		/// </summary>
		/// <param name="dataReader">The parameter that references the <see cref="IDataReader"/></param>
		/// <param name="ordinal">The parameter that contains the ordinal of the column to read</param>
		/// <returns>An expression for reading the column into a value</returns>
		public abstract Expression GetReadExpression(Expression dataReader, int ordinal);

		public virtual Expression IsNullExpression(Expression dataReader, int ordinal)
		{
			return Expression.Call(dataReader, IsDbNullMethod, Expression.Constant(ordinal));
		}
	}
}
