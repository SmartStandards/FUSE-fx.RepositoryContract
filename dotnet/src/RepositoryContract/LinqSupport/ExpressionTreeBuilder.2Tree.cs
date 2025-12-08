using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Fuse.LinqSupport {

  internal static partial class ExpressionTreeMapper {

    /// <summary>
    /// Builds a FUSE ExpressionTree from a given lambda predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the predicate.</typeparam>
    /// <param name="predicate">The LINQ expression to translate.</param>
    /// <returns>A corresponding ExpressionTree instance.</returns>
    public static ExpressionTree BuildTreeFromLinqExpression<TEntity>(Expression<Func<TEntity, bool>> predicate) {
      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      Expression body = predicate.Body;
      Expression normalized = RemoveConvert(body);

      ExpressionTree tree = BuildExpressionTree(normalized);

      return tree;
    }

    /// <summary>
    /// Recursively builds an ExpressionTree from a boolean expression.
    /// </summary>
    /// <param name="expression">The expression to translate.</param>
    /// <returns>The resulting ExpressionTree.</returns>
    private static ExpressionTree BuildExpressionTree(Expression expression) {
      Expression cleaned = RemoveConvert(expression);

      BinaryExpression binaryExpression = cleaned as BinaryExpression;
      if (binaryExpression != null) {
        if (binaryExpression.NodeType == ExpressionType.AndAlso ||
            binaryExpression.NodeType == ExpressionType.OrElse) {
          return BuildLogicalGroup(binaryExpression);
        }

        FieldPredicate atomicPredicate;
        if (TryBuildFieldPredicateFromBinary(binaryExpression, out atomicPredicate)) {
          ExpressionTree atomicTree = new ExpressionTree();
          atomicTree.MatchAll = true;
          atomicTree.Negate = false;
          atomicTree.Predicates = new List<FieldPredicate>();
          atomicTree.Predicates.Add(atomicPredicate);
          atomicTree.SubTree = null;
          return atomicTree;
        }

        throw new NotSupportedException("Unsupported binary expression in predicate: " + binaryExpression.NodeType.ToString());
      }

      UnaryExpression unaryExpression = cleaned as UnaryExpression;
      if (unaryExpression != null) {
        if (unaryExpression.NodeType == ExpressionType.Not) {
          ExpressionTree innerTree = BuildExpressionTree(unaryExpression.Operand);
          innerTree.Negate = !innerTree.Negate;
          return innerTree;
        }

        throw new NotSupportedException("Unsupported unary expression in predicate: " + unaryExpression.NodeType.ToString());
      }

      MethodCallExpression methodCallExpression = cleaned as MethodCallExpression;
      if (methodCallExpression != null) {
        FieldPredicate methodPredicate;
        if (TryBuildFieldPredicateFromMethodCall(methodCallExpression, out methodPredicate)) {
          ExpressionTree methodTree = new ExpressionTree();
          methodTree.MatchAll = true;
          methodTree.Negate = false;
          methodTree.Predicates = new List<FieldPredicate>();
          methodTree.Predicates.Add(methodPredicate);
          methodTree.SubTree = null;
          return methodTree;
        }

        throw new NotSupportedException("Unsupported method call in predicate: " + methodCallExpression.Method.Name);
      }

      MemberExpression memberExpression = cleaned as MemberExpression;
      if (memberExpression != null && cleaned.Type == typeof(bool)) {
        FieldPredicate predicate = BuildFieldPredicateFromBooleanMember(memberExpression, true);

        ExpressionTree memberTree = new ExpressionTree();
        memberTree.MatchAll = true;
        memberTree.Negate = false;
        memberTree.Predicates = new List<FieldPredicate>();
        memberTree.Predicates.Add(predicate);
        memberTree.SubTree = null;

        return memberTree;
      }

      ConstantExpression constantExpression = cleaned as ConstantExpression;
      if (constantExpression != null && constantExpression.Type == typeof(bool)) {
        bool value = (bool)constantExpression.Value;

        if (value) {
          ExpressionTree trueTree = ExpressionTree.Empty();
          return trueTree;
        }
        else {
          ExpressionTree falseTree = ExpressionTree.Empty();
          falseTree.Negate = true;
          return falseTree;
        }
      }

      throw new NotSupportedException("Unsupported expression type in predicate: " + cleaned.NodeType.ToString());
    }

    /// <summary>
    /// Builds a logical AND/OR group ExpressionTree for a BinaryExpression with AndAlso/OrElse.
    /// </summary>
    /// <param name="binaryExpression">The logical binary expression.</param>
    /// <returns>The resulting ExpressionTree.</returns>
    private static ExpressionTree BuildLogicalGroup(BinaryExpression binaryExpression) {
      bool matchAll = binaryExpression.NodeType == ExpressionType.AndAlso;

      ExpressionTree groupTree = new ExpressionTree();
      groupTree.MatchAll = matchAll;
      groupTree.Negate = false;
      groupTree.Predicates = new List<FieldPredicate>();
      groupTree.SubTree = new List<ExpressionTree>();

      AddOperandToGroup(groupTree, binaryExpression.Left);
      AddOperandToGroup(groupTree, binaryExpression.Right);

      if (groupTree.SubTree != null && groupTree.SubTree.Count == 0) {
        groupTree.SubTree = null;
      }

      return groupTree;
    }

    /// <summary>
    /// Adds an operand expression to a logical group, either as atomic predicate or as nested subtree.
    /// </summary>
    /// <param name="groupTree">The parent logical group.</param>
    /// <param name="operand">The operand expression.</param>
    private static void AddOperandToGroup(ExpressionTree groupTree, Expression operand) {
      Expression cleaned = RemoveConvert(operand);

      FieldPredicate atomicPredicate;
      if (TryBuildFieldPredicate(cleaned, out atomicPredicate)) {
        groupTree.Predicates.Add(atomicPredicate);
        return;
      }

      ExpressionTree subTree = BuildExpressionTree(cleaned);
      groupTree.SubTree.Add(subTree);
    }

    /// <summary>
    /// Tries to build a FieldPredicate directly from a given expression.
    /// Supported are binary comparisons, string methods and boolean member access.
    /// </summary>
    /// <param name="expression">Expression to inspect.</param>
    /// <param name="predicate">Resulting predicate if successful.</param>
    /// <returns>True if a predicate could be created, otherwise false.</returns>
    private static bool TryBuildFieldPredicate(Expression expression, out FieldPredicate predicate) {
      predicate = null;

      Expression cleaned = RemoveConvert(expression);

      BinaryExpression binaryExpression = cleaned as BinaryExpression;
      if (binaryExpression != null) {
        if (TryBuildFieldPredicateFromBinary(binaryExpression, out predicate)) {
          return true;
        }

        return false;
      }

      MethodCallExpression methodCallExpression = cleaned as MethodCallExpression;
      if (methodCallExpression != null) {
        if (TryBuildFieldPredicateFromMethodCall(methodCallExpression, out predicate)) {
          return true;
        }

        return false;
      }

      MemberExpression memberExpression = cleaned as MemberExpression;
      if (memberExpression != null && cleaned.Type == typeof(bool)) {
        predicate = BuildFieldPredicateFromBooleanMember(memberExpression, true);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Tries to build a FieldPredicate from a binary comparison expression.
    /// </summary>
    /// <param name="binaryExpression">The binary expression.</param>
    /// <param name="predicate">The resulting predicate.</param>
    /// <returns>True if the expression could be translated; otherwise false.</returns>
    private static bool TryBuildFieldPredicateFromBinary(BinaryExpression binaryExpression, out FieldPredicate predicate) {
      predicate = null;

      Expression left = RemoveConvert(binaryExpression.Left);
      Expression right = RemoveConvert(binaryExpression.Right);

      MemberExpression leftMember = left as MemberExpression;
      MemberExpression rightMember = right as MemberExpression;

      bool leftIsField = IsMemberAccessOnParameter(leftMember);
      bool rightIsField = IsMemberAccessOnParameter(rightMember);

      if (leftIsField && rightIsField) {
        return false;
      }

      Expression fieldExpression = null;
      Expression valueExpression = null;

      bool valueDeclarationIsReversed = false;
      if (leftIsField && right.NodeType == ExpressionType.Constant) {
        fieldExpression = leftMember;
        valueExpression = right;
      }
      else if (rightIsField && left.NodeType == ExpressionType.Constant) {
        fieldExpression = rightMember;
        valueExpression = left;
        valueDeclarationIsReversed = true;
      }
      else {
        return false;
      }

      string fieldName = ExtractMemberPath((MemberExpression)fieldExpression);
      object value = GetValueFromExpression(valueExpression);

      switch (binaryExpression.NodeType) {
        case ExpressionType.Equal:
          predicate = FieldPredicate.Equal(fieldName, value);
          return true;

        case ExpressionType.NotEqual:
          predicate = FieldPredicate.NotEqual(fieldName, value);
          return true;

        case ExpressionType.GreaterThan:
          if (valueDeclarationIsReversed) {
            predicate = FieldPredicate.Less(fieldName, value);
          }
          else {
            predicate = FieldPredicate.Greater(fieldName, value);
          }
          return true;

        case ExpressionType.GreaterThanOrEqual:
          if (valueDeclarationIsReversed) {
            predicate = FieldPredicate.LessOrEqual(fieldName, value);
          }
          else {
            predicate = FieldPredicate.GreaterOrEqual(fieldName, value);
          }
          return true;

        case ExpressionType.LessThan:
          if (valueDeclarationIsReversed) {
            predicate = FieldPredicate.Greater(fieldName, value);
          }
          else {
            predicate = FieldPredicate.Less(fieldName, value);
          }
          return true;

        case ExpressionType.LessThanOrEqual:
          if (valueDeclarationIsReversed) {
            predicate = FieldPredicate.GreaterOrEqual(fieldName, value);
          }
          else {
            predicate = FieldPredicate.LessOrEqual(fieldName, value);
          }
          return true;
      }

      return false;
    }

    /// <summary>
    /// Tries to build a FieldPredicate from a method call expression
    /// (string.Contains, string.StartsWith, string.EndsWith, Enumerable.Contains for "IN").
    /// </summary>
    /// <param name="methodCallExpression">The method call expression.</param>
    /// <param name="predicate">Resulting predicate.</param>
    /// <returns>True if the expression could be translated; otherwise false.</returns>
    private static bool TryBuildFieldPredicateFromMethodCall(MethodCallExpression methodCallExpression, out FieldPredicate predicate) {
      predicate = null;

      if (methodCallExpression.Method.Name == "Contains") {
        if (methodCallExpression.Object != null) {
          if (methodCallExpression.Object.Type == typeof(string)) {
            MemberExpression fieldMember = methodCallExpression.Object as MemberExpression;
            if (IsMemberAccessOnParameter(fieldMember)) {
              string fieldName = ExtractMemberPath(fieldMember);
              object value = GetValueFromExpression(methodCallExpression.Arguments[0]);
              predicate = FieldPredicate.Contains(fieldName, value);
              return true;
            }
          }
        }
        else {
          if (methodCallExpression.Arguments.Count == 2) {
            Expression collectionExpression = methodCallExpression.Arguments[0];
            Expression valueExpression = methodCallExpression.Arguments[1];

            MemberExpression fieldMember = valueExpression as MemberExpression;
            if (IsMemberAccessOnParameter(fieldMember)) {
              string fieldName = ExtractMemberPath(fieldMember);
              object collectionValue = GetValueFromExpression(collectionExpression);
              object arrayValue = ConvertToObjectArray(collectionValue);

              FieldPredicate inPredicate = new FieldPredicate();
              inPredicate.FieldName = fieldName;
              inPredicate.Operator = FieldOperators.In;
              inPredicate.Value = arrayValue;

              predicate = inPredicate;
              return true;
            }
          }
        }
      }

      if (methodCallExpression.Object != null && methodCallExpression.Object.Type == typeof(string)) {
        MemberExpression stringMember = methodCallExpression.Object as MemberExpression;
        if (IsMemberAccessOnParameter(stringMember)) {
          string fieldName = ExtractMemberPath(stringMember);
          object argumentValue = null;

          if (methodCallExpression.Arguments != null && methodCallExpression.Arguments.Count > 0) {
            argumentValue = GetValueFromExpression(methodCallExpression.Arguments[0]);
          }

          if (methodCallExpression.Method.Name == "StartsWith") {
            predicate = FieldPredicate.StartsWith(fieldName, argumentValue);
            return true;
          }

          if (methodCallExpression.Method.Name == "EndsWith") {
            FieldPredicate endsWithPredicate = new FieldPredicate();
            endsWithPredicate.FieldName = fieldName;
            endsWithPredicate.Operator = FieldOperators.EndsWith;
            endsWithPredicate.Value = argumentValue;
            predicate = endsWithPredicate;
            return true;
          }

          if (methodCallExpression.Method.Name == "Contains") {
            predicate = FieldPredicate.Contains(fieldName, argumentValue);
            return true;
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Builds a predicate for a boolean member access like "x => x.IsActive".
    /// This is translated into "Field == true".
    /// </summary>
    /// <param name="memberExpression">The member expression.</param>
    /// <param name="expectedValue">The boolean value to compare against.</param>
    /// <returns>A FieldPredicate representing the boolean comparison.</returns>
    private static FieldPredicate BuildFieldPredicateFromBooleanMember(MemberExpression memberExpression, bool expectedValue) {
      string fieldName = ExtractMemberPath(memberExpression);
      FieldPredicate predicate = FieldPredicate.Equal(fieldName, expectedValue);
      return predicate;
    }

    /// <summary>
    /// Removes Convert/ConvertChecked unary wrappers from an expression.
    /// </summary>
    /// <param name="expression">The expression to normalize.</param>
    /// <returns>The underlying expression without pure conversion nodes.</returns>
    private static Expression RemoveConvert(Expression expression) {
      Expression current = expression;

      while (current is UnaryExpression) {
        UnaryExpression unaryExpression = (UnaryExpression)current;
        if (unaryExpression.NodeType == ExpressionType.Convert ||
            unaryExpression.NodeType == ExpressionType.ConvertChecked) {
          current = unaryExpression.Operand;
        }
        else {
          break;
        }
      }

      return current;
    }

    /// <summary>
    /// Determines whether the given member expression ultimately points to the lambda parameter
    /// (e.g. x.Property or x.Sub.Property).
    /// </summary>
    /// <param name="memberExpression">The member expression.</param>
    /// <returns>True if the access is rooted at a parameter expression.</returns>
    private static bool IsMemberAccessOnParameter(MemberExpression memberExpression) {
      if (memberExpression == null) {
        return false;
      }

      Expression current = memberExpression.Expression;

      while (current is MemberExpression) {
        MemberExpression innerMember = (MemberExpression)current;
        current = innerMember.Expression;
      }

      ParameterExpression parameterExpression = current as ParameterExpression;
      return parameterExpression != null;
    }

    /// <summary>
    /// Extracts a dot-separated member path from a member expression
    /// (e.g. "x.Address.City" becomes "Address.City").
    /// </summary>
    /// <param name="memberExpression">The member expression.</param>
    /// <returns>The extracted member path.</returns>
    private static string ExtractMemberPath(MemberExpression memberExpression) {
      List<string> parts = new List<string>();
      Expression current = memberExpression;

      while (current is MemberExpression) {
        MemberExpression currentMember = (MemberExpression)current;
        parts.Insert(0, currentMember.Member.Name);
        current = currentMember.Expression;
      }

      string path = string.Join(".", parts.ToArray());
      return path;
    }

    /// <summary>
    /// Evaluates an expression to an object value.
    /// Supports constants and arbitrary expressions via a small compiled lambda.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The evaluated value.</returns>
    private static object GetValueFromExpression(Expression expression) {
      ConstantExpression constantExpression = expression as ConstantExpression;
      if (constantExpression != null) {
        return constantExpression.Value;
      }

      Expression converted = Expression.Convert(expression, typeof(object));
      Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(converted);
      Func<object> compiled = lambda.Compile();
      object value = compiled();
      return value;
    }

    /// <summary>
    /// Converts a collection value to an object[] array to be used with FieldOperators.In.
    /// </summary>
    /// <param name="collectionValue">The collection object.</param>
    /// <returns>An object[] containing the collection items.</returns>
    private static object ConvertToObjectArray(object collectionValue) {
      if (collectionValue == null) {
        return new object[0];
      }

      if (collectionValue is string) {
        object[] singleItemArray = new object[1];
        singleItemArray[0] = collectionValue;
        return singleItemArray;
      }

      IEnumerable enumerable = collectionValue as IEnumerable;
      if (enumerable == null) {
        object[] single = new object[1];
        single[0] = collectionValue;
        return single;
      }

      List<object> items = new List<object>();
      IEnumerator enumerator = enumerable.GetEnumerator();
      while (enumerator.MoveNext()) {
        items.Add(enumerator.Current);
      }

      object[] result = items.ToArray();
      return result;
    }

  }

}
