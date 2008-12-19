// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ContainsParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public ContainsParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 2, parseContext.ExpressionTreeRoot);
        ParserUtility.CheckParameterType<Expression> (methodCallExpression, "Contains", 0, parseContext.ExpressionTreeRoot);
        //return CreateContains ((SubQueryExpression) methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], parseContext);
        return CreateContains (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], parseContext);
      }
      throw ParserUtility.CreateParserException ("Contains with expression", methodCallExpression.Method.Name,
          "method call expression in where condition", parseContext.ExpressionTreeRoot);
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
          return true;
      }
      return false;
    }

    private BinaryCondition CreateContains (Expression expression, Expression itemExpression, ParseContext parseContext)
    {
      //SubQuery subQuery = new SubQuery (subQueryExpression.QueryModel, null);
      return new BinaryCondition (
        _parserRegistry.GetParser (expression).Parse (expression, parseContext),
        _parserRegistry.GetParser (itemExpression).Parse (itemExpression, parseContext),
          BinaryCondition.ConditionKind.Contains);
    }

    //private BinaryCondition CreateContains (SubQueryExpression subQueryExpression, Expression itemExpression, ParseContext parseContext)
    //{
    //  SubQuery subQuery = new SubQuery (subQueryExpression.QueryModel, null);
    //  return new BinaryCondition (
    //    subQuery, 
    //    _parserRegistry.GetParser (itemExpression).Parse (itemExpression, parseContext),
    //      BinaryCondition.ConditionKind.Contains);
    //}
  }
}