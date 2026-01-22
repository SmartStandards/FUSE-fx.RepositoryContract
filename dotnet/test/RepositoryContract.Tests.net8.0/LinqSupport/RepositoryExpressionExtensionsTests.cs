using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Data.Fuse.LinqSupport {

  [TestClass]
  public class RepositoryExpressionTreeBuilderTests {

    private class Person {
      public string Name { get; set; }
      public int Age { get; set; }
      public bool IsActive { get; set; }
      public string Country { get; set; }
    }

    // --------------------------------------------------------------------
    // Helper: Extract first predicate for simple expressions
    // --------------------------------------------------------------------

    private FieldPredicate SinglePredicate(ExpressionTree tree) {
      Assert.IsNotNull(tree);
      Assert.IsNotNull(tree.Predicates);
      Assert.AreEqual(1, tree.Predicates.Count);
      return tree.Predicates[0];
    }

    private FieldPredicate FirstPredicate(ExpressionTree tree) {
      Assert.IsNotNull(tree);
      Assert.IsNotNull(tree.Predicates);
      return tree.Predicates[0];
    }

    // --------------------------------------------------------------------
    // EQUAL
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_EqualOperator_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age == 30;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.Equal, pred.Operator);
      Assert.AreEqual(30, pred.Value);
    }

    [TestMethod]
    public void LinqSupp_EqualOperatorWirthVariable_IsTranslated() {
      string name = "Karl";

      Expression<Func<Person, bool>> expr = (p) => p.Name == name && p.IsActive;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = FirstPredicate(tree);
      Assert.AreEqual("Name", pred.FieldName);
      Assert.AreEqual(FieldOperators.Equal, pred.Operator);
      Assert.AreEqual(name, pred.Value);
    }

    [TestMethod]
    public void LinqSupp_EqualOperatorWirthVariableNull_IsTranslated() {
      string name = null;

      Expression<Func<Person, bool>> expr = (p) => p.Name == name && p.IsActive;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = FirstPredicate(tree);
      Assert.AreEqual("Name", pred.FieldName);
      Assert.AreEqual(FieldOperators.Equal, pred.Operator);
      Assert.AreEqual(name, pred.Value);
    }

    // --------------------------------------------------------------------
    // NOT EQUAL
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_NotEqualOperator_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age != 10;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.NotEqual, pred.Operator);
      Assert.AreEqual(10, pred.Value);
    }

    // --------------------------------------------------------------------
    // GREATER
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_GreaterThan_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age > 18;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.Greater, pred.Operator);
      Assert.AreEqual(18, pred.Value);
    }

    // --------------------------------------------------------------------
    // GREATER OR EQUAL
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_GreaterOrEqual_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age >= 21;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.GreaterOrEqual, pred.Operator);
      Assert.AreEqual(21, pred.Value);
    }

    // --------------------------------------------------------------------
    // LESS THAN
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_LessThan_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age < 99;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.Less, pred.Operator);
      Assert.AreEqual(99, pred.Value);
    }

    // --------------------------------------------------------------------
    // LESS OR EQUAL
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_LessOrEqual_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Age <= 40;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Age", pred.FieldName);
      Assert.AreEqual(FieldOperators.LessOrEqual, pred.Operator);
      Assert.AreEqual(40, pred.Value);
    }

    // --------------------------------------------------------------------
    // STRING CONTAINS
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_StringContains_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Name.Contains("abc");
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Name", pred.FieldName);
      Assert.AreEqual(FieldOperators.Contains, pred.Operator);
      Assert.AreEqual("abc", pred.Value);
    }

    // --------------------------------------------------------------------
    // STARTSWITH
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_StartsWith_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Name.StartsWith("Jo");
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Name", pred.FieldName);
      Assert.AreEqual(FieldOperators.StartsWith, pred.Operator);
      Assert.AreEqual("Jo", pred.Value);
    }

    // --------------------------------------------------------------------
    // ENDSWITH
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_EndsWith_IsTranslated() {
      Expression<Func<Person, bool>> expr = p => p.Name.EndsWith("son");
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Name", pred.FieldName);
      Assert.AreEqual(FieldOperators.EndsWith, pred.Operator);
      Assert.AreEqual("son", pred.Value);
    }

    // --------------------------------------------------------------------
    // IN (Enumerable.Contains)
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_EnumerableContains_IsTranslatedTo_In() {
      string[] countries = new[] { "DE", "AT", "CH" };

      Expression<Func<Person, bool>> expr =
          p => countries.Contains(p.Country);

      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Country", pred.FieldName);
      Assert.AreEqual(FieldOperators.In, pred.Operator);

      object[] arr = pred.Value as object[];
      Assert.IsNotNull(arr);
      Assert.AreEqual(3, arr.Length);
    }

    [TestMethod]
    public void LinqSupp_EnumerableContains_IsTranslatedTo_In_EmptyArray() {
      string[] countries = new string[] {};

      Expression<Func<Person, bool>> expr =
          p => countries.Contains(p.Country);

      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("Country", pred.FieldName);
      Assert.AreEqual(FieldOperators.In, pred.Operator);

      object[] arr = pred.Value as object[];
      Assert.IsNotNull(arr);
      Assert.AreEqual(0, arr.Length);
    }

    // --------------------------------------------------------------------
    // BOOL MEMBER (x.IsActive)
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_BoolMember_IsTranslatedTo_EqualTrue() {
      Expression<Func<Person, bool>> expr = p => p.IsActive;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("IsActive", pred.FieldName);
      Assert.AreEqual(FieldOperators.Equal, pred.Operator);
      Assert.AreEqual(true, pred.Value);
    }

    // --------------------------------------------------------------------
    // NOT 
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_NotExpression_FlipsNegate() {
      Expression<Func<Person, bool>> expr = p => !p.IsActive;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual("IsActive", pred.FieldName);
      Assert.AreEqual(FieldOperators.Equal, pred.Operator);
      Assert.IsTrue(tree.Negate);
    }

    // --------------------------------------------------------------------
    // AND / OR
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_AndAlso_Builds_MatchAll_Tree() {
      Expression<Func<Person, bool>> expr = p => p.Age > 10 && p.Age < 20;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      Assert.IsTrue(tree.MatchAll);
      Assert.IsNotNull(tree.Predicates);
      Assert.AreEqual(2, tree.Predicates.Count);
    }

    [TestMethod]
    public void LinqSupp_OrElse_Builds_MatchAny_Tree() {
      Expression<Func<Person, bool>> expr = p => p.Age < 5 || p.Age > 50;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      Assert.IsFalse(tree.MatchAll);
      Assert.IsNotNull(tree.Predicates);
      Assert.AreEqual(2, tree.Predicates.Count);
    }

    // --------------------------------------------------------------------
    // COMPLEX NESTING
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_ComplexExpression_IsCorrectlyNested() {
      Expression<Func<Person, bool>> expr =
          p => (p.Age > 18 && p.Name.StartsWith("A"))
              || (!p.IsActive && p.Country == "DE");

      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      Assert.IsFalse(tree.MatchAll); // top-level OR
      Assert.IsNotNull(tree.SubTree);
      Assert.AreEqual(2, tree.SubTree.Count);
    }

    // --------------------------------------------------------------------
    // CONSTANT TRUE / FALSE
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_ConstantTrue_Returns_EmptyTree() {
      Expression<Func<Person, bool>> expr = p => true;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      Assert.IsNotNull(tree);
      Assert.IsFalse(tree.Negate);
    }

    [TestMethod]
    public void LinqSupp_ConstantFalse_Returns_NegatedTree() {
      Expression<Func<Person, bool>> expr = p => false;
      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      Assert.IsTrue(tree.Negate);
    }

    // --------------------------------------------------------------------
    // CONVERT
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_Convert_WrappedExpressions_AreIgnored() {
      Expression<Func<Person, bool>> expr =
          p => (int)p.Age > 10;

      ExpressionTree tree = ExpressionTreeMapper.BuildTreeFromLinqExpression(expr);

      FieldPredicate pred = SinglePredicate(tree);
      Assert.AreEqual(FieldOperators.Greater, pred.Operator);
      Assert.AreEqual(10, pred.Value);
    }

  }

}
