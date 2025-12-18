using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Fuse.Convenience.Aggregation {

  /// <summary>
  /// Evaluates ExpressionTree on arbitrary POCOs via reflection.
  /// It implements ExpressionTree semantics including:
  /// - MatchAll (AND/OR)
  /// - Negate
  /// - SubTrees
  /// - Duplicate predicates on the same field in AND context treated as OR per field.
  /// </summary>
  public static class LinqEvaluator {
    public static bool Matches(object entity, ExpressionTree filter) {
      if (filter == null) {
        return true;
      }

      bool result;

      if (filter.MatchAll) {
        result = MatchesAndSemantics(entity, filter);
      }
      else {
        result = MatchesOrSemantics(entity, filter);
      }

      if (filter.Negate) {
        result = !result;
      }

      return result;
    }

    private static bool MatchesAndSemantics(object entity, ExpressionTree filter) {
      // AND across:
      // - each field group: OR within group if duplicate field predicates exist
      // - each subtree: AND
      Dictionary<string, List<FieldPredicate>> grouped = GroupPredicatesByField(filter);

      foreach (KeyValuePair<string, List<FieldPredicate>> kvp in grouped) {
        bool groupOk = false;

        List<FieldPredicate> group = kvp.Value;
        int i = 0;
        while (i < group.Count) {
          if (MatchesPredicate(entity, group[i])) {
            groupOk = true;
            break;
          }
          i++;
        }

        if (!groupOk) {
          return false;
        }
      }

      if (filter.SubTree != null && filter.SubTree.Count > 0) {
        int s = 0;
        while (s < filter.SubTree.Count) {
          if (!Matches(entity, filter.SubTree[s])) {
            return false;
          }
          s++;
        }
      }

      // If there were no predicates and no subtrees, the empty AND tree matches all.
      return true;
    }

    private static bool MatchesOrSemantics(object entity, ExpressionTree filter) {
      // OR across:
      // - any predicate
      // - any subtree
      if (filter.Predicates != null && filter.Predicates.Count > 0) {
        int i = 0;
        while (i < filter.Predicates.Count) {
          if (MatchesPredicate(entity, filter.Predicates[i])) {
            return true;
          }
          i++;
        }
      }

      if (filter.SubTree != null && filter.SubTree.Count > 0) {
        int s = 0;
        while (s < filter.SubTree.Count) {
          if (Matches(entity, filter.SubTree[s])) {
            return true;
          }
          s++;
        }
      }

      // Empty OR tree matches none.
      return false;
    }

    private static Dictionary<string, List<FieldPredicate>> GroupPredicatesByField(ExpressionTree filter) {
      Dictionary<string, List<FieldPredicate>> grouped = new Dictionary<string, List<FieldPredicate>>(StringComparer.OrdinalIgnoreCase);

      if (filter.Predicates == null || filter.Predicates.Count == 0) {
        return grouped;
      }

      int i = 0;
      while (i < filter.Predicates.Count) {
        FieldPredicate p = filter.Predicates[i];

        string key = p.FieldName;
        if (string.IsNullOrEmpty(key)) {
          key = string.Empty;
        }

        List<FieldPredicate> list;
        if (!grouped.TryGetValue(key, out list)) {
          list = new List<FieldPredicate>();
          grouped[key] = list;
        }

        list.Add(p);
        i++;
      }

      return grouped;
    }

    private static bool MatchesPredicate(object entity, FieldPredicate predicate) {
      if (predicate == null) {
        return true;
      }

      object left = GetFieldValue(entity, predicate.FieldName);
      object right = predicate.Value;

      string op = predicate.Operator;

      if (string.Equals(op, FieldOperators.Equal, StringComparison.OrdinalIgnoreCase)) {
        return AreEqual(left, right);
      }
      if (string.Equals(op, FieldOperators.NotEqual, StringComparison.OrdinalIgnoreCase)) {
        return !AreEqual(left, right);
      }
      if (string.Equals(op, FieldOperators.Greater, StringComparison.OrdinalIgnoreCase)) {
        return Compare(left, right) > 0;
      }
      if (string.Equals(op, FieldOperators.GreaterOrEqual, StringComparison.OrdinalIgnoreCase)) {
        return Compare(left, right) >= 0;
      }
      if (string.Equals(op, FieldOperators.Less, StringComparison.OrdinalIgnoreCase)) {
        return Compare(left, right) < 0;
      }
      if (string.Equals(op, FieldOperators.LessOrEqual, StringComparison.OrdinalIgnoreCase)) {
        return Compare(left, right) <= 0;
      }
      if (string.Equals(op, FieldOperators.Contains, StringComparison.OrdinalIgnoreCase)) {
        if (left == null || right == null) {
          return false;
        }

        string ls = Convert.ToString(left);
        string rs = Convert.ToString(right);
        if (ls == null || rs == null) {
          return false;
        }

        return ls.IndexOf(rs, StringComparison.OrdinalIgnoreCase) >= 0;
      }
      if (string.Equals(op, FieldOperators.In, StringComparison.OrdinalIgnoreCase)) {
        if (right == null) {
          return false;
        }

        IEnumerable enumerable = right as IEnumerable;
        if (enumerable == null) {
          return false;
        }

        foreach (object item in enumerable) {
          if (AreEqual(left, item)) {
            return true;
          }
        }

        return false;
      }

      throw new NotSupportedException("Operator not supported in LinqEvaluator: " + op);
    }

    private static object GetFieldValue(object entity, string fieldName) {
      if (entity == null) {
        return null;
      }
      if (string.IsNullOrEmpty(fieldName)) {
        return null;
      }

      Type t = entity.GetType();

      PropertyInfo p = t.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
      if (p != null) {
        return p.GetValue(entity, null);
      }

      FieldInfo f = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
      if (f != null) {
        return f.GetValue(entity);
      }

      return null;
    }

    private static bool AreEqual(object a, object b) {
      if (a == null && b == null) {
        return true;
      }
      if (a == null || b == null) {
        return false;
      }

      if (a.Equals(b)) {
        return true;
      }

      // Try numeric conversion
      try {
        decimal da = Convert.ToDecimal(a);
        decimal db = Convert.ToDecimal(b);
        return da == db;
      }
      catch {
        // ignore conversion
      }

      return false;
    }

    private static int Compare(object a, object b) {
      if (a == null || b == null) {
        throw new InvalidOperationException("Cannot compare null values.");
      }

      try {
        decimal da = Convert.ToDecimal(a);
        decimal db = Convert.ToDecimal(b);

        if (da < db) {
          return -1;
        }
        if (da > db) {
          return 1;
        }
        return 0;
      }
      catch {
        // Fallback to string compare
        string sa = Convert.ToString(a);
        string sb = Convert.ToString(b);

        return string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
      }
    }

  }

}
