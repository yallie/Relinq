using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Structure;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class SourceExpressionParserTest
  {
    private SourceExpressionParser _topLevelParser;
    private SourceExpressionParser _notTopLevelParser;
    private IQueryable<Student> _source;
    private ParameterExpression _potentialFromIdentifier;

    [SetUp]
    public void SetUp()
    {
      _topLevelParser = new SourceExpressionParser (true);
      _notTopLevelParser = new SourceExpressionParser (false);
      _source = ExpressionHelper.CreateQuerySource();
      _potentialFromIdentifier = ExpressionHelper.CreateParameterExpression();
    }

    [Test]
    public void NonDistinct ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsFalse (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeSelect()
    {
      IQueryable<string> query = TestQueryGenerator.CreateSimpleDisinctQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeWhere ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateDisinctWithWhereQueryWithoutProjection (_source);
      ParseResultCollector result = Parse(query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Distinct is only allowed at the top level of a query, not in the middle: "
        + "'value(Rubicon.Data.Linq.UnitTests.TestQueryable`1[Rubicon.Data.Linq.UnitTests.Student]).Select(s => s).Distinct().Where(s => s.IsOld)'.")]
    public void Distinct_NonTopLevel ()
    {
      IQueryable<Student> query = _source.Select (s => s).Distinct().Where (s => s.IsOld);
      Parse (query);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Constant or Call expression for xy, found ParameterExpression (i).")]
    public void InvalidSource ()
    {
      _topLevelParser.Parse (new ParseResultCollector (_source.Expression), _potentialFromIdentifier, _potentialFromIdentifier, "xy");
    }

    [Test]
    public void SimpleSource()
    {
      IQueryable<Student> query = _source;
      ParseResultCollector result = Parse (query);
      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (((FromExpression) result.BodyExpressions[0]).Identifier, _potentialFromIdentifier);
      Assert.AreEqual (((FromExpression) result.BodyExpressions[0]).Expression, query.Expression);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    public void Select ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectExpressionParser().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void SelectMany ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateMultiFromQuery (_source, _source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectManyExpressionParser ().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void Where_TopLevel ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery(_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new WhereExpressionParser (true).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void Where_NotTopLevel ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (_source);
      ParseResultCollector result = Parse_NotTopLevel (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new WhereExpressionParser (false).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void OrderBy_TopLevel ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new OrderByExpressionParser (true).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void OrderBy_NotTopLevel ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (_source);
      ParseResultCollector result = Parse_NotTopLevel (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new OrderByExpressionParser (false).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    private void AssertResultsEqual(ParseResultCollector one, ParseResultCollector two)
    {
      Assert.AreEqual (one.ExpressionTreeRoot, two.ExpressionTreeRoot);
      Assert.AreEqual (one.IsDistinct, two.IsDistinct);
      Assert.AreEqual (one.ProjectionExpressions.Count, two.ProjectionExpressions.Count);
      Assert.AreEqual (one.BodyExpressions.Count, two.BodyExpressions.Count);

      for (int i = 0; i < one.ProjectionExpressions.Count; ++i)
        Assert.AreEqual (one.ProjectionExpressions[i], two.ProjectionExpressions[i]);

      for (int i = 0; i < one.BodyExpressions.Count; ++i)
      {
        FromExpression fromExpression1 = one.BodyExpressions[i] as FromExpression;
        WhereExpression whereExpression1 = one.BodyExpressions[i] as WhereExpression;
        OrderExpression orderExpression1 = one.BodyExpressions[i] as OrderExpression;
        if (fromExpression1 != null)
        {
          FromExpression fromExpression2 = two.BodyExpressions[i] as FromExpression;

          Assert.AreEqual (fromExpression1.Identifier, fromExpression2.Identifier);
          Assert.AreEqual (fromExpression1.Expression, fromExpression2.Expression);
        }
        else if (whereExpression1 != null)
        {
          WhereExpression whereExpression2 = two.BodyExpressions[i] as WhereExpression;
          Assert.AreEqual (whereExpression1.Expression, whereExpression2.Expression);
        }
        else
        {
          OrderExpression orderExpression2 = two.BodyExpressions[i] as OrderExpression;
          Assert.AreEqual (orderExpression1.Expression, orderExpression2.Expression);
        }
      }
    }

    private ParseResultCollector Parse (IQueryable query)
    {
      ParseResultCollector result = new ParseResultCollector (query.Expression);
      _topLevelParser.Parse (result, query.Expression, _potentialFromIdentifier, "bla");
      return result;
    }

    private ParseResultCollector Parse_NotTopLevel (IQueryable query)
    {
      ParseResultCollector result = new ParseResultCollector (query.Expression);
      _notTopLevelParser.Parse (result, query.Expression, _potentialFromIdentifier, "bla");
      return result;
    }

  }
}