using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;
using System.Text;
using System.Text.Json;

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
        return CompileRelationToDynamicLinq(expression.AtomArguments[0]);
      }

      List<string> childResults = new List<string>();
      foreach (RelationElement relationElement in expression.AtomArguments) {
        childResults.Add(CompileRelationToDynamicLinq(relationElement));
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

    private static string CompileRelationToDynamicLinq(RelationElement relationElement) {

      //TODO in and not in in oder usw übersetzen!
      string[] inRels = new string[] { "in" };
      if (inRels.Contains(relationElement.Relation)) {
        JsonElement valuesJson = (JsonElement)relationElement.Value;
        foreach (var value in valuesJson.EnumerateArray()) {

        }
        IEnumerable test = valuesJson.EnumerateArray();
        int x = 0;
      }

      string serializedValue;
      string propertyName = relationElement.PropertyName + " ";
      string relation = relationElement.Relation;

      string[] ineqRels = new string[] { "!=", "<>", "isnot", "is not", "!==", "Isnot", "IsNot", "Is not", "Is Not" };
      if (ineqRels.Contains(relation)) {
        relation = "!=";
      }

      bool checkNull = false;
      string[] notNullRels = new string[] { "exists", "Exists", "has value", "HasValue", "not null", "!= null", "is not null" };
      if (notNullRels.Contains(relation)) {
        relation = "!= null";
        checkNull = true;
      }
      string[] isNullRels = new string[] { "!exists", "not exists", "is null", "== null" };
      if (isNullRels.Contains(relation)) {
        relation = "== null";
        checkNull = true;
      }


      if (checkNull) {
        serializedValue = "";
      }
      else {

        switch (relationElement.PropertyType) {
          case "string":
          case "String":
            serializedValue = $"\"{relationElement.Value}\"";
            string[] containsRelations = new string[] { ">", ">=", "contains", "Contains", "includes", "Includes" };
            string[] reversContainsRelations = new string[] { "<", "<=", "is substring of", "substring", "substringOf" }; //TODO
            if (containsRelations.Contains(relation)) {
              propertyName = relationElement.PropertyName;
              relation = ".Contains";
              serializedValue = $"(\"{relationElement.Value}\")";
            }
            if (containsRelations.Contains(relation)) {
              propertyName = $"(\"{relationElement.Value}\")";
              relation = ".Contains";
              serializedValue = relationElement.PropertyName;
            }
            break;
          case "dateTime":
          case "DateTime":
          case "datetime":
          case "Datetime":
            DateTime date = DateTime.Parse(relationElement.Value.ToString());
            serializedValue = $"DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}, {date.Millisecond})";
            break;
          case "date":
          case "Date":
            DateTime date2 = DateTime.Parse(relationElement.Value.ToString());
            serializedValue = $"DateTime({date2.Year}, {date2.Month}, {date2.Day}).Date";
            propertyName = $"{relationElement.PropertyName}.Date ";
            break;
          default:
            serializedValue = relationElement.Value.ToString();
            break;
        }
      }

      StringBuilder result = new StringBuilder();
      result.Append("(");
      result.Append(propertyName);
      result.Append(relation);
      result.Append(serializedValue);
      result.Append(")");
      return result.ToString();
    }
  }
}