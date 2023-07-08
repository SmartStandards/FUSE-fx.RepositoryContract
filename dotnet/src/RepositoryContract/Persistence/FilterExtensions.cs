using System;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text;

namespace System.Data.Fuse {
  public static class FilterExtensions {

    public static string CompileToDynamicLinq(this SimpleExpressionTree tree) {
      if (tree == null) return null;
      if (tree.RootNode == null) return null;
      return tree.RootNode.CompileToDynamicLinq();
    }

    public static string CompileToDynamicLinq(this LogicalExpression expression) {

      if (expression == null) { return ""; }

      if (expression.Operator == "") {
        if (expression.AtomArguments?.Count != 1) { return ""; }
        return CompileRelation(expression.AtomArguments[0]);
      }

      List<string> childResults = new List<string>();
      foreach (RelationElement relationElement in expression.AtomArguments) {
        childResults.Add(CompileRelation(relationElement));
      }
      foreach (LogicalExpression ex in expression.ExpressionArguments) {
        childResults.Add(CompileToDynamicLinq(ex));
      }

      string dlOperator;
      switch (expression.Operator) {
        case "and":
        case "And":
        case "und":
        case "Und":
        case "&&":
        case "&":
          dlOperator = "and";
          break;
        default:
          return "";

      }

      StringBuilder result = new StringBuilder();
      result.Append("(");
      foreach (string childResult in childResults) {
        result.Append(childResult);
        result.Append(" " + dlOperator + " ");
      }
      result.Append(")");
      return result.ToString();
    }

    private static string CompileRelation(RelationElement relationElement) {

      string serializedValue;
      switch (relationElement.PropertyType) {
        case "string":
        case "String":
          serializedValue = $"\"{relationElement.Value}\"";
          break;
        default:
          serializedValue = relationElement.Value.ToString();
          break;
      }

      StringBuilder result = new StringBuilder();
      result.Append("(");
      result.Append(relationElement.PropertyName + " ");
      result.Append(relationElement.Relation);
      result.Append(serializedValue);
      result.Append(")");
      return result.ToString();
    }
  }
}