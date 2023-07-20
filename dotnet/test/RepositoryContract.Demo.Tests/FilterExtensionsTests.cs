using System.Data.Fuse;
using System.Data.Fuse.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RepositoryContract.Demo.Tests {
  [TestClass]
  public class FilterExtensionsTests {
    [TestMethod]
    public void CompileToSqlWhere_SimpleString_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "FirstName",
          PropertyType = "string",
          Relation = "=",
          Value = "John"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("(FirstName = 'John')", result);
    }

    [TestMethod]
    public void CompileToSqlWhere_StringContains_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "FirstName",
          PropertyType = "string",
          Relation = ">=",
          Value = "Joh"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("(FirstName  like  '%Joh%')", result);
    }

    [TestMethod]
    public void CompileToSqlWhere_StringReverseContains_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "FirstName",
          PropertyType = "string",
          Relation = "<=",
          Value = "Joh"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("('Joh'  like  '%'+FirstName+'%')", result);
    }

    [TestMethod]
    public void CompileToSqlWhere_SimpleDateTime_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "DateOfBirth",
          PropertyType = "datetime",
          Relation = "<=",
          Value = "2023-07-19T14:30:00"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("(DateOfBirth <= '2023-7-19T14:30:0.0')", result);
    }

    [TestMethod]
    public void CompileToSqlWhere_SimpleDate_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "DateOfBirth",
          PropertyType = "date",
          Relation = "<=",
          Value = "2023-07-19T14:30:00"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("(DateOfBirth <= '2023-7-19')", result);
    }

    [TestMethod]
    public void CompileToSqlWhere_Or_Works() {

      SimpleExpressionTree tree = new SimpleExpressionTree();
      tree.RootNode = new LogicalExpression();
      tree.RootNode.Operator = "Or";
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "DateOfBirth",
          PropertyType = "date",
          Relation = "<=",
          Value = "2023-07-19T14:30:00"
        }
      );
      tree.RootNode.AtomArguments.Add(
        new RelationElement() {
          PropertyName = "DateOfBirth",
          PropertyType = "date",
          Relation = ">=",
          Value = "2023-07-19T14:30:00"
        }
      );

      string result = tree.CompileToSqlWhere();
      Assert.AreEqual("((DateOfBirth <= '2023-7-19') or (DateOfBirth >= '2023-7-19'))", result);
    }
  }
}