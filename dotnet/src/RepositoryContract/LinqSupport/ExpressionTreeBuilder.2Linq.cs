using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.LinqSupport {

  internal static partial class ExpressionTreeMapper {

    /// <summary>
    /// Rebuilds a LINQ Expression&lt;Func&lt;TEntity,bool&gt;&gt; from a FUSE ExpressionTree.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildLinqExpressionFromTree<TEntity>(ExpressionTree tree) {
      if (tree == null) {
        throw new ArgumentNullException("tree");
      }

      ParameterExpression param = Expression.Parameter(typeof(TEntity), "p");

      Expression body = BuildExpressionFromNode(tree, param);

      if (body == null) {
        body = Expression.Constant(true);
      }

      return Expression.Lambda<Func<TEntity, bool>>(body, param);
    }

    //=====================================================================
    //  Rebuild tree recursively
    //=====================================================================
    private static Expression BuildExpressionFromNode(ExpressionTree node, ParameterExpression param) {
      Expression baseExpression = null;

      List<Expression> elements = new List<Expression>();

      // Field predicates → atomic expressions
      if (node.Predicates != null) {
        for (int i = 0; i < node.Predicates.Count; i++) {
          FieldPredicate pred = node.Predicates[i];
          Expression expr = BuildPredicateExpression(pred, param);
          elements.Add(expr);
        }
      }

      // Sub-trees → recursive group expressions
      if (node.SubTree != null) {
        for (int i = 0; i < node.SubTree.Count; i++) {
          Expression sub = BuildExpressionFromNode(node.SubTree[i], param);
          elements.Add(sub);
        }
      }

      // No content → return TRUE
      if (elements.Count == 0) {
        baseExpression = Expression.Constant(true);
      }
      else {
        baseExpression = elements[0];

        for (int i = 1; i < elements.Count; i++) {
          if (node.MatchAll) {
            baseExpression = Expression.AndAlso(baseExpression, elements[i]);
          }
          else {
            baseExpression = Expression.OrElse(baseExpression, elements[i]);
          }
        }
      }

      // Apply negation
      if (node.Negate) {
        baseExpression = Expression.Not(baseExpression);
      }

      return baseExpression;
    }

    //=====================================================================
    // Build one predicate: FieldOperators.* + IN + string ops
    //=====================================================================
    private static Expression BuildPredicateExpression(FieldPredicate predicate, ParameterExpression param) {
      string fieldName = predicate.FieldName;

      Expression member = BuildMemberAccess(param, fieldName);

      object constantValue = predicate.Value;
      Expression constExpr = Expression.Constant(constantValue);

      switch (predicate.Operator) {
        case FieldOperators.Equal:
          return Expression.Equal(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.NotEqual:
          return Expression.NotEqual(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.Greater:
          return Expression.GreaterThan(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.GreaterOrEqual: // + FieldOperators.Contains        
          if(param.Type == typeof(string)) {
            return BuildStringCall(member, "Contains", constantValue);
          }
          return Expression.GreaterThanOrEqual(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.Less:
          return Expression.LessThan(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.LessOrEqual:
          return Expression.LessThanOrEqual(member, Expression.Convert(constExpr, member.Type));

        case FieldOperators.StartsWith:
          return BuildStringCall(member, "StartsWith", constantValue);

        case FieldOperators.EndsWith:
          return BuildStringCall(member, "EndsWith", constantValue);

        case FieldOperators.In:
          return BuildInOperator(member, constantValue);

        default:
          throw new NotSupportedException("Unsupported FieldOperator: " + predicate.Operator.ToString());
      }
    }

    //=====================================================================
    //  Member Access: "Address.City" → p.Address.City
    //=====================================================================
    private static Expression BuildMemberAccess(ParameterExpression param, string path) {
      string[] parts = path.Split('.');

      Expression current = param;

      for (int i = 0; i < parts.Length; i++) {
        PropertyInfo prop = current.Type.GetProperty(parts[i]);
        if (prop == null) {
          throw new InvalidOperationException(
              "Property not found: " + parts[i] + " on type " + current.Type.FullName
          );
        }

        current = Expression.Property(current, prop);
      }

      return current;
    }

    //=====================================================================
    //  String methods
    //=====================================================================
    private static Expression BuildStringCall(Expression member, string method, object value) {
      if (member.Type != typeof(string)) {
        throw new InvalidOperationException(
            "String operator '" + method + "' used on non-string member " + member.ToString()
        );
      }

      MethodInfo mi = typeof(string).GetMethod(method, new Type[] { typeof(string) });

      return Expression.Call(member, mi, Expression.Constant(value, typeof(string)));
    }

    //=====================================================================
    //  IN operator →   array.Contains(member)
    //=====================================================================
    private static Expression BuildInOperator(Expression member, object value) {
      object[] array = value as object[];

      if (array == null) {
        throw new InvalidOperationException("IN operator expects an object[] array.");
      }

      Expression arrayExpr = Expression.Constant(array);

      MethodInfo containsMethod =
          typeof(Enumerable).GetMethods()
          .Where(m => m.Name == "Contains" && m.GetParameters().Length == 2)
          .First()
          .MakeGenericMethod(member.Type);

      Expression convertedArray = Expression.Constant(
          array.Select(x => Convert.ChangeType(x, member.Type)).ToArray(),
          typeof(IEnumerable<>).MakeGenericType(member.Type)
      );

      return Expression.Call(null, containsMethod, convertedArray, member);
    }

  }

}
