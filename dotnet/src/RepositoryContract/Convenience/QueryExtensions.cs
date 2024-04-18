using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.ModelDescription;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
#if NETCOREAPP
using System.Text.Json;
using static System.Text.Json.JsonElement;
#endif

namespace System.Data.Fuse.Convenience {

  public static class QueryExtensions {

    public static string CompileToDynamicLinq(this ExpressionTree tree, EntitySchema entitySchema) {
      if (tree == null) return null;
      return tree.CompileToWhereStatement(entitySchema, "dynamic linq", "");
    }

    public static string CompileToSqlWhere(this ExpressionTree tree, EntitySchema entitySchema, string prefix = "") {
      if (tree == null) return null;
      return tree.CompileToWhereStatement(entitySchema, "sql", prefix);
    }

    internal static string CompileToWhereStatement(
      this ExpressionTree expression,
      EntitySchema entitySchema,
      string mode,
      string prefix
    ) {

      if (expression == null) { return "1=1"; }

      List<string> childResults = new List<string>();
      foreach (FieldPredicate fieldPredicate in expression.Predicates) {
        childResults.Add(CompileFieldPredicateToWhereStatement(entitySchema, fieldPredicate, mode, prefix));
      }

      if (expression.SubTree != null) {
        foreach (ExpressionTree ex in expression.SubTree) {
          childResults.Add(CompileToWhereStatement(ex, entitySchema, mode, prefix));
        }
      }

      string dlOperator = expression.MatchAll ? "and" : "or";

      StringBuilder result = new StringBuilder();
      if (dlOperator == "not") {
        result.Append("(");
        result.Append("not ");
      }

      if (childResults.Count() == 0) {
        return "1=1";
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

      if (dlOperator == "not") {
        result.Append(")");
      }

      return result.ToString();
    }

    internal static string CompileFieldPredicateToWhereStatement(
      EntitySchema entitySchema, FieldPredicate relationElement, string mode, string prefix
    ) {

      string[] inRels = new string[] { FieldOperators.In };
      if (inRels.Contains(relationElement.Operator)) {
        StringBuilder result1 = new StringBuilder();
        result1.Append("(");
        object rawValues = relationElement.Value;
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(rawValues?.GetType())) {
          JsonElement valuesJson = (JsonElement)relationElement.Value;
          ArrayEnumerator values = valuesJson.EnumerateArray();
          foreach (JsonElement value in values) {
            FieldPredicate innerRelationElement = new FieldPredicate() {
              FieldName = relationElement.FieldName,
              Operator = "=",
              Value = value
            };
            result1.Append(CompileFieldPredicateToWhereStatement(entitySchema, innerRelationElement, mode, prefix));
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
            FieldPredicate innerRelationElement = new FieldPredicate() {
              FieldName = relationElement.FieldName,
              Operator = "=",
              Value = value
            };
            result1.Append(CompileFieldPredicateToWhereStatement(entitySchema, innerRelationElement, mode, prefix));
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

      //      string[] notInRels = new string[] { "not in" };
      //      if (notInRels.Contains(relationElement.Relation)) {
      //        StringBuilder result1 = new StringBuilder();
      //        result1.Append("(");
      //        object rawValues = relationElement.Value;
      //#if NETCOREAPP
      //        if (typeof(JsonElement).IsAssignableFrom(rawValues?.GetType())) {
      //          JsonElement valuesJson = (JsonElement)relationElement.Value;
      //          ArrayEnumerator values = valuesJson.EnumerateArray();
      //          foreach (JsonElement value in values) {
      //            RelationElement innerRelationElement = new RelationElement() {
      //              PropertyName = relationElement.PropertyName,
      //              PropertyType = relationElement.PropertyType,
      //              Relation = "!=",
      //              Value = value
      //            };
      //            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
      //            result1.Append(" and ");
      //          }
      //          if (values.Count() > 0) {
      //            result1.Length -= 5;
      //          }
      //        } else {
      //#endif
      //          IEnumerable values = (IEnumerable)rawValues;
      //          int count = 0;
      //          foreach (object value in values) {
      //            count++;
      //            RelationElement innerRelationElement = new RelationElement() {
      //              PropertyName = relationElement.PropertyName,
      //              PropertyType = relationElement.PropertyType,
      //              Relation = "!=",
      //              Value = value
      //            };
      //            result1.Append(CompileRelationToWhereStatement(innerRelationElement, mode, prefix));
      //            result1.Append(" and ");
      //          }
      //          if (count > 0) {
      //            result1.Length -= 5;
      //          }
      //#if NETCOREAPP
      //        }
      //#endif
      //        result1.Append(")");
      //        return result1.ToString();
      //      }

      string serializedValue;
      string fieldName = prefix + relationElement.FieldName;
      string @operator = relationElement.Operator;

      string[] ineqRels = new string[] { FieldOperators.NotEqual, "!=", "<>", "isnot", "is not", "!==", "Isnot", "IsNot", "Is not", "Is Not" };
      if (ineqRels.Contains(@operator)) {
        @operator = "!=";
      }

      string[] eqRels = new string[] { FieldOperators.Equal, "==", "=", "is", "===", "equals" };
      if (eqRels.Contains(@operator)) {
        @operator = "=";
      }

      bool checkNull = false;
      string[] notNullRels = new string[] { "exists", "Exists", "has value", "HasValue", "not null", "!= null", "is not null" };
      if (notNullRels.Contains(@operator)) {
        if (mode == "sql") {
          @operator = "is not null";
        } else {
          @operator = "!= null";
        }
        checkNull = true;
      }
      string[] isNullRels = new string[] { "!exists", "not exists", "is null", "== null" };
      if (isNullRels.Contains(@operator)) {
        if (mode == "sql") {
          @operator = "is null";
        } else {
          @operator = "== null";
        }
        checkNull = true;
      }
      string fieldType = "string";
      
      FieldSchema fieldSchema = entitySchema.Fields.FirstOrDefault((f) => f.Name == relationElement.FieldName);
      if (fieldSchema != null) {
        fieldType = fieldSchema.Type;
      }
      if (checkNull) {
        serializedValue = "";
      } else {   
        if (
          (fieldType == "DateTime" || fieldType == "Date") &&
          DateTime.TryParse(relationElement.Value.ToString(), out DateTime dateTime)
        ) {
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
        } else if (fieldType == "String" || fieldType == "Guid") {
          serializedValue = ResolveStringField(relationElement, mode, prefix, ref fieldName, ref @operator);
        } else {
          serializedValue = relationElement.Value.ToString();
        }

        //switch (relationElement.PropertyType) {
        //  case "string":
        //  case "String":
        //    if (mode == "sql") {
        //      serializedValue = $"'{relationElement.Value}'";
        //    } else {
        //      serializedValue = $"\"{relationElement.Value}\"";
        //    }
        //    string[] containsRelations = new string[] { ">", ">=", "contains", "Contains", "includes", "Includes" };
        //    string[] reversContainsRelations = new string[] { "<", "<=", "is substring of", "substring", "substringOf" };
        //    if (containsRelations.Contains(@operator)) {
        //      fieldName = prefix + relationElement.PropertyName;
        //      if (mode == "sql") {
        //        @operator = " like ";
        //        serializedValue = $"'%{relationElement.Value}%'";
        //      } else {
        //        @operator = ".Contains";
        //        serializedValue = $"(\"{relationElement.Value}\")";
        //      }
        //    }
        //    if (reversContainsRelations.Contains(@operator)) {
        //      if (mode == "sql") {
        //        fieldName = $"'{relationElement.Value}'";
        //        @operator = " like ";
        //        serializedValue = $"'%'+{prefix + relationElement.PropertyName}+'%'";
        //      } else {
        //        fieldName = $"\"{relationElement.Value}\"";
        //        @operator = ".Contains";
        //        serializedValue = $"({prefix + relationElement.PropertyName})";
        //      }
        //    }
        //    break;
        //  case "dateTime":
        //  case "DateTime":
        //  case "datetime":
        //  case "Datetime":
        //    DateTime date = DateTime.Parse(relationElement.Value.ToString());
        //    if (mode == "sql") {
        //      if (date.Hour > 9) {
        //        serializedValue = $"'{date.Year}-{date.Month}-{date.Day}T{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond}'";
        //      } else {
        //        serializedValue = $"'{date.Year}-{date.Month}-{date.Day}T0{date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond}'";
        //      }
        //    } else {
        //      serializedValue = $"DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}, {date.Millisecond})";
        //    }
        //    break;
        //  case "date":
        //  case "Date":
        //    DateTime date2 = DateTime.Parse(relationElement.Value.ToString());
        //    if (mode == "sql") {
        //      serializedValue = $"'{date2.Year}-{date2.Month}-{date2.Day}'";
        //      fieldName = prefix + relationElement.PropertyName;
        //    } else {
        //      serializedValue = $"DateTime({date2.Year}, {date2.Month}, {date2.Day}).Date";
        //      fieldName = $"{prefix + relationElement.PropertyName}.Date ";
        //    }
        //    break;
        //  default:
        //    serializedValue = relationElement.Value.ToString();
        //    break;
        //}

      }

      StringBuilder result = new StringBuilder();
      result.Append("(");
      result.Append(fieldName);
      result.Append(" ");
      result.Append(@operator);
      result.Append(" ");
      result.Append(serializedValue);
      result.Append(")");

      if (mode == "sql" && fieldType != "string" && fieldType != "String") {
      result.Replace("\"", "'");
      }

      return result.ToString();
    }

    private static string ResolveStringField(FieldPredicate fieldPredicate, string mode, string prefix, ref string fieldName, ref string @operator) {
      string serializedValue;
      if (mode == "sql") {
        serializedValue = $"'{fieldPredicate.Value}'";
      } else {
        serializedValue = $"\"{fieldPredicate.Value}\"";
      }
      string[] containsRelations = new string[] { ">", ">=", "contains", "Contains", "includes", "Includes" };
      string[] reversContainsRelations = new string[] { "<", "<=", "is substring of", "substring", "substringOf" };
      if (containsRelations.Contains(@operator)) {
        fieldName = prefix + fieldPredicate.FieldName;
        if (mode == "sql") {
          @operator = " like ";
          serializedValue = $"'%{fieldPredicate.Value}%'";
        } else {
          @operator = ".Contains";
          serializedValue = $"(\"{fieldPredicate.Value}\")";
        }
      }
      if (reversContainsRelations.Contains(@operator)) {
        if (mode == "sql") {
          fieldName = $"'{fieldPredicate.Value}'";
          @operator = " like ";
          serializedValue = $"'%'+{prefix + fieldPredicate.FieldName}+'%'";
        } else {
          fieldName = $"\"{fieldPredicate.Value}\"";
          @operator = ".Contains";
          serializedValue = $"({prefix + fieldPredicate.FieldName})";
        }
      }

      return serializedValue;
    }

    public static void DeleteEntities(
      object[][] entityIdsToDelete,
      Func<PropertyInfo[]> getKeyProperties,
      Action<object[]> deleteEntityByKeyset
    ) {
      foreach (object[] entityIdToDelete in entityIdsToDelete) {
        if (entityIdsToDelete.Length == 0) {
          continue;
        }
        object[] keysetToDelete = new object[entityIdsToDelete.Length];
        PropertyInfo[] keyProperties = getKeyProperties();
        if (keyProperties.Count() != keysetToDelete.Count()) { continue; }
        int j = 0;
        foreach (PropertyInfo keyProperty in keyProperties) {
          object keyPropValue = entityIdToDelete[j];
#if NETCOREAPP
          if (keyPropValue != null && typeof(JsonElement).IsAssignableFrom(keyPropValue.GetType())) {
            JsonElement keyPropValueJson = (JsonElement)keyPropValue;
            keyPropValue = ConversionExtensions.GetValue(keyProperty, keyPropValueJson);
          }
#endif
          keysetToDelete[j] = keyPropValue;
        }
        deleteEntityByKeyset.Invoke(keysetToDelete);
      }
    }

    public static object[] GetValues(this object entity, List<PropertyInfo> properties) {
      List<object> values = new List<object>();
      foreach (PropertyInfo propertyInfo in properties) {
        values.Add(propertyInfo.GetValue(entity, null));
      }
      return values.ToArray();
    }

    public static TEntity FindMatch<TEntity>(
      this IList<TEntity> entities, TEntity entity, PropertyInfo[] properties
    ) {
      return entities.FirstOrDefault(entity.GetSearchExpression(properties).Compile());
    }

    public static TEntity FindMatchByValues<TEntity>(
      this IList<TEntity> entities, object[] values, PropertyInfo[] properties
    ) {
      return entities.FirstOrDefault(values.GetSearchExpressionByValues<TEntity>(properties).Compile());
    }

    public static Expression<Func<TEntity, bool>> GetSearchExpression<TEntity>(
      this TEntity entity, params PropertyInfo[] properties
    ) {
      var parameter = Expression.Parameter(typeof(TEntity), "e");
      Expression body = null;

      foreach (PropertyInfo propertyInfo in properties) {
        var entityValue = propertyInfo.GetValue(entity);
        var equality = Expression.Equal(
            Expression.Property(parameter, propertyInfo.Name),
            Expression.Constant(entityValue)
        );

        body = body == null ? equality : Expression.AndAlso(body, equality);
      }

      if (body == null)
        throw new ArgumentException("At least one property must be provided");

      return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    public static Expression<Func<TEntity, bool>> GetSearchExpressionByValues<TEntity>(
      this object[] values, params PropertyInfo[] properties
    ) {
      if (values.Length != properties.Length)
        throw new ArgumentException("Number of values must match number of properties");

      var parameter = Expression.Parameter(typeof(TEntity), "e");
      Expression body = null;

      for (int i = 0; i < properties.Length; i++) {
        var propertyInfo = properties[i];
        var value = values[i];
        var equality = Expression.Equal(
            Expression.Property(parameter, propertyInfo.Name),
            Expression.Constant(value)
        );

        body = body == null ? equality : Expression.AndAlso(body, equality);
      }

      if (body == null)
        throw new ArgumentException("At least one property must be provided");

      return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }


    public static object[] TryGetValuesByFields(
      this Dictionary<string, object> fields,
      List<PropertyInfo> properties
    ) {
      List<object> values = new List<object>();
      foreach (PropertyInfo propertyInfo in properties) {
        if (!fields.ContainsKey(propertyInfo.Name)) {
          return null;
        }
        values.Add(propertyInfo.GetValue(fields[propertyInfo.Name], null));
      }
      return values.ToArray();
    }

    public static object[] GetKeyFieldValues(this object key) {
      if (key is ICompositeKey compositeKey) {
        return compositeKey.GetFields();
      } else {
        return new object[] { key };
      }
    }

    public static TKey ToKey<TKey>(this object[] fieldValues) {
      if (typeof(ICompositeKey).IsAssignableFrom(typeof(TKey))) {
        return (TKey)Activator.CreateInstance(typeof(TKey), new object[] { fieldValues });
      } else {
        return (TKey)fieldValues[0];
      }
    }

    public static Expression<Func<TEntity, bool>> BuildFilterForKeyValuesExpression<TEntity, TKey>(
      this TKey[] valuesToLoad, PropertyInfo[] propertyInfos
    ) {
      var parameter = Expression.Parameter(typeof(TEntity), "entity");
      Expression body = Expression.Constant(false);

      foreach (var key in valuesToLoad) {
        object[] keyValues;
        if (key is ICompositeKey compositeKey) {
          keyValues = compositeKey.GetFields();
        } else {
          keyValues = new object[] { key };
        }

        if (keyValues.Length != propertyInfos.Length) {
          throw new ArgumentException("Number of key values must match number of properties");
        }

        Expression keyExpression = Expression.Constant(true);

        for (int i = 0; i < propertyInfos.Length; i++) {
          var property = propertyInfos[i];
          var value = Expression.Constant(keyValues[i]);
          keyExpression = Expression.AndAlso(
              keyExpression, Expression.Equal(Expression.Property(parameter, property.Name), value)
          );
        }

        body = Expression.OrElse(body, keyExpression);
      }

      var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
      return lambda;
    }

  }
}