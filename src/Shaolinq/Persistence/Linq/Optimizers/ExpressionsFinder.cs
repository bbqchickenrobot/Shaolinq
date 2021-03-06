﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq.Optimizers
{
	public class ExpressionsFinder
		: SqlExpressionVisitor
	{
		private readonly bool findFirst;
		private readonly List<Expression> results;
		private readonly Predicate<Expression> isMatch;
		
		private ExpressionsFinder(Predicate<Expression> isMatch, bool findFirst)
		{
			this.isMatch = isMatch;
			this.findFirst = findFirst;

			this.results = new List<Expression>();
		}

		public static bool FindExists(Expression expression, Predicate<Expression> isMatch)
		{
			var finder = new ExpressionsFinder(isMatch, true);

			finder.Visit(expression);

			return finder.results.Count > 0;
		}

		public static Expression First(Expression expression, Predicate<Expression> isMatch)
		{
			var finder = new ExpressionsFinder(isMatch, true);

			finder.Visit(expression);

			return finder.results.FirstOrDefault();
		}

		public static List<Expression> FindAll(Expression expression, Predicate<Expression> isMatch)
		{
			var finder = new ExpressionsFinder(isMatch, false);

			finder.Visit(expression);

			return finder.results;
		}

		protected override Expression Visit(Expression expression)
		{
			if (this.findFirst && this.results.Count > 0)
			{
				return expression;
			}

			if (expression == null)
			{
				return base.Visit(null);
			}

			if (this.isMatch(expression))
			{
				this.results.Add(expression);

				if (this.findFirst)
				{
					return expression;
				}
				
				return base.Visit(expression);
			}

			return base.Visit(expression);
		}
	}
}
