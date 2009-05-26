// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class GroupClauseTest
  {
    [Test]
    public void InitializeWithGroupExpressionAndByExpression()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression();
      Expression byExpression = ExpressionHelper.CreateExpression();

      IClause clause = ExpressionHelper.CreateClause();
      GroupClause groupClause = new GroupClause (clause, groupExpression, byExpression);

      Assert.AreSame (clause, groupClause.PreviousClause);
      Assert.AreSame (groupExpression, groupClause.GroupExpression);
      Assert.AreSame (byExpression, groupClause.ByExpression);
    }

    [Test]
    public void GroupClause_ImplementISelectGroupClause ()
    {
      GroupClause groupClause = ExpressionHelper.CreateGroupClause();

      Assert.IsInstanceOfType (typeof (ISelectGroupClause), groupClause);
    }

    [Test]
    public void Accept()
    {
      GroupClause groupClause = ExpressionHelper.CreateGroupClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor>();
      visitorMock.VisitGroupClause (groupClause);

      repository.ReplayAll();

      groupClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var originalClause = ExpressionHelper.CreateGroupClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.ByExpression, Is.SameAs (originalClause.ByExpression));
      Assert.That (clone.GroupExpression, Is.SameAs (originalClause.GroupExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }
  }
}
