using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;
using System.Text;
#if NETCOREAPP
using System.Text.Json;
using static System.Text.Json.JsonElement;
#endif

namespace System.Data.Fuse {
  public static class FilterExtensions {

    public static string CompileToDynamicLinq(this LogicalExpression tree) {
      if (tree == null) return null;
      return tree.CompileToWhereStatement("dynamic linq", "");
    }

    public static string CompileToSqlWhere(this LogicalExpression tree, string prefix = "") {
      if (tree == null) return null;
      return tree.CompileToWhereStatement("sql", prefix);
    }

    internal static string CompileToWhereStatement(this LogicalExpression expression, string mode, string prefix) {

      if (expression == null) { return ""; }

      if (expression.Operator == "") {
        int numberOfArguments = expression.AtomArguments.Count + expression.ExpressionArguments.Count;
        if (numberOfArguments != 1) { return ""; }
        if (expression.AtomArguments.Count == 1) {
          return CompileRelationToWhereStatement(expression.AtomArguments[0], mode, prefix);
        } else {
          return CompileToWhereStatement(expression.ExpressionArguments[0], mode, prefix);
        }
      }

      List<string> childResults = new List<string>();
      foreach (RelationElement relationElement in expression.AtomArguments) {
        childResults.Add(CompileRelationToWhereStatement(relationElement, mode, prefix));
      }
      foreach (LogicalExpression ex in expression.ExpressionArguments) {
        childResults.Add(CompileToWhereStatement(ex, mode, prefix));
      }

      string dlOperator;
      switch (expression.Operator) {
        case "not":
        case "Not":
        case "NOT":
        case "!":
          dlOperator = "not";
          break;
        case "and":
        case "And":
        case "und":
        case "Und":
        case "&&":
        case "&":
          dlOperator = "and";
          break;
        case "or":
        case "Or":
        case "oder":
        case "Oder":
        case "||":
        case "|":
          dlOperator = "or";
          break;
        default:
          return "";

      }
      StringBuilder result = new StringBuilder();
      if (dlOperator == "not") {
        if (childResults.Count > 1 || childResults.Count == 0) {
          return "";
        }
        result.Append(dlOperator);
        result.Append("");
        result.Append("(");
        result.Append(childResults[0]);
        result.Append(")");
        return result.ToString();
      }


      result.Append("(");
      foreach (string childResult in childResults) {
        result.Append(childResult);
        result.Append(" " + dlOperator + " ");
      }
      if (childResults.Count() > 0) {
        result.Length -= (2 + dlOperator.Length);
      }
      result.Append(")");
      return result.ToString();
    }

    internal static string CompileRelationToWhereStatement(
      RelationElement relationElement, string mode, string prefix
    ) {

      string[] inRels = new string[] { "in" };
      if (inRels.Contains(relationElement.Relation)) {
        StringBuilder result1 = new StringBuilder();
        result1.Append("(");
        object rawValues = relationElement.Value;
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(rawValues?.GetType())) {
          JsonElement valuesJson = (JsonElement)relationElement.Value;
          ArrayEnumerator values = valuesJson.EnumerateArray();
          foreach (JsonElement value in values) {
            RelationElement innerRelationElement = new RelationElement() {
              PropertyName = relationElement.PropertyName,
              PropertyType = relationElement.PropertyType,
              Relation = "=",
              Value = value
            };
            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
            result1.Append(" or ");
          }
          if (values.Count() > 0) {
            result1.Length -= 4;
          }
        } else {
#endif
          IEnumerable values = (IEnumerable)rawValues;
          int count = 0;
          foreach (object value in values) {
            count++;
            RelationElement innerRelationElement = new RelationElement() {
              PropertyName = relationElement.PropertyName,
              PropertyType = relationElement.PropertyType,
              Relation = "=",
              Value = value
            };
            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
            result1.Append(" or ");
          }
          if (count > 0) {
            result1.Length -= 4;
          }
#if NETCOREAPP
        }
#endif
        result1.Append(")");
        return result1.ToString();
      }

      string[] notInRels = new string[] { "not in" };
      if (notInRels.Contains(relationElement.Relation)) {
        StringBuilder result1 = new StringBuilder();
        result1.Append("(");
        object rawValues = relationElement.Value;
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(rawValues?.GetType())) {
          JsonElement valuesJson = (JsonElement)relationElement.Value;
          ArrayEnumerator values = valuesJson.EnumerateArray();
          foreach (JsonElement value in values) {
            RelationElement innerRelationElement = new RelationElement() {
              PropertyName = relationElement.PropertyName,
              PropertyType = relationElement.PropertyType,
              Relation = "!=",
              Value = value
            };
            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
            result1.Append(" and ");
          }
          if (values.Count() > 0) {
            result1.Length -= 5;
          }
        } else {
#endif
          IEnumerable values = (IEnumerable)rawValues;
          int count = 0;
          foreach (object value in values) {
            count++;
            RelationElement innerRelationElement = new RelationElement() {
              PropertyName = relationElement.PropertyName,
              PropertyType = relationElement.PropertyType,
              Relation = "!=",
              Value = value
            };
            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
            result1.Append(" and ");
          }
          if (count > 0) {
            result1.Length -= 5;
          }
#if NETCOREAPP
        }
#endif
        result1.Append(")");
        return result1.ToString();
      }

      string serializedValue;
      string propertyName = prefix + relationElement.PropertyName;
      string relation = relationElement.Relation;

      string[] ineqRels = new string[] { "!=", "<>", "isnot", "is not", "!==", "Isnot", "IsNot", "Is not", "Is Not" };
      if (ineqRels.Contains(relation)) {
        relation = "!=";
      }

      string[] eqRels = new string[] { "==", "=", "is", "===", "equals" };
      if (eqRels.Contains(relation)) {
        relation = "=";
      }

      bool checkNull = false;
      string[] notNullRels = new string[] { "exists", "Exists", "has value", "HasValue", "not null", "!= null", "is not null" };
      if (notNullRels.Contains(relation)) {
        if (mode == "sql") {
          relation = "is not null";
        } else {
          relation = "!= null";
        }
        checkNull = true;
      }
      string[] isNullRels = new string[] { "!exists", "not exists", "is null", "== null" };
      if (isNullRels.Contains(relation)) {
        if (mode == "sql") {
          relation = "is null";
        } else {
          relation = "== null";
        }
        checkNull = true;
      }


      if (checkNull) {
        serializedValue = "";
      } else {

        switch (relationElement.PropertyType) {
          case "string":
          case "String":
            if (mode == "sql") {
              serializedValue = $"'{relationElement.Value}'";
            } else {
              serializedValue = $"\"{relationElement.Value}\"";
            }
            string[] containsRelations = new string[] { ">", ">=", "contains", "Contains", "includes", "Includes" };
            string[] reversContainsRelations = new string[] { "<", "<=", "is substring of", "substring", "substringOf" };
            if (containsRelations.Contains(relation)) {
              propertyName = prefix + relationElement.PropertyName;
              if (mode == "sql") {
                relation = " like ";
                serializedValue = $"'%{relationElement.Value}%'";
              } else {
                relation = ".Contains";
                serializedValue = $"(\"{relationElement.Value}\")";
              }
            }
            if (reversContainsRelations.Contains(relation)) {
              if (mode == "sql") {
                propertyName = $"'{relationElement.Value}'";
                relation = " like ";
                serializedValue = $"'%'+{prefix + relationElement.PropertyName}+'%'";
              } else {
                propertyName = $"\"{relationElement.Value}\"";
                relation = ".Contains";
                serializedValue = $"({prefix + relationElement.PropertyName})";
              }
            }
            break;
          case "dateTime":
          case "DateTime":
          case "datetime":
          case "Datetime":
            DateTime date = DateTime.Parse(relationElement.Value.ToString());
            if (mode == "sql") {
              if (date.Hour > 9) {
                serializedValue = $"'{date.Year}-{date.Month}-{date.Day}T{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond}'";
              } else {
                serializedValue = $"'{date.Year}-{date.Month}-{date.Day}T0{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond}'";
              }
            } else {
              serializedValue = $"DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}, {date.Millisecond})";
            }
            break;
          case "date":
          case "Date":
            DateTime date2 = DateTime.Parse(relationElement.Value.ToString());
            if (mode == "sql") {
              serializedValue = $"'{date2.Year}-{date2.Month}-{date2.Day}'";
              propertyName = prefix + relationElement.PropertyName;
            } else {
              serializedValue = $"DateTime({date2.Year}, {date2.Month}, {date2.Day}).Date";
              propertyName = $"{prefix + relationElement.PropertyName}.Date ";
            }
            break;
          default:
            serializedValue = relationElement.Value.ToString();
            break;
        }
      }

      StringBuilder result = new StringBuilder();
      result.Append("(");
      result.Append(propertyName);
      result.Append(" ");
      result.Append(relation);
      result.Append(" ");
      result.Append(serializedValue);
      result.Append(")");

      if (mode == "sql" && relationElement.PropertyType != "string" && relationElement.PropertyType != "String") {
        result.Replace("\"", "'");
      }

      return result.ToString();
    }
  }
}