using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Data.Fuse.Convenience.Internal;
using System.Data.ModelDescription;
using System;

namespace System.Data.Fuse.Convenience.Aggregation { 

  /// <summary>
  /// Splits an ExpressionTree into pushdown filters per source and a residual filter.
  /// Correctness guarantee:
  /// For any entity, originalFilter(entity) == (PrimaryPushdown AND SecondaryPushdown AND Residual)(entity)
  /// using ExpressionTree semantics including:
  /// - MatchAll (AND/OR)
  /// - Negate
  /// - Subtrees
  /// - Duplicate predicates on same field in AND context treated as OR per field
  /// </summary>
  public static class ExpressionTreeSplitter {

    /// <summary>
    /// Splits a filter using routing information.
    /// </summary>
    public static ExpressionTreeSplitResult Split(ExpressionTree filter, PredicateRoutingMap routing) {
      if (routing == null) {
        throw new ArgumentNullException(nameof(routing));
      }

      ExpressionTree safeFilter = filter;
      if (safeFilter == null) {
        safeFilter = ExpressionTree.Empty();
      }

      ExpressionTree primary = ExpressionTree.Empty();
      ExpressionTree secondary = ExpressionTree.Empty();
      ExpressionTree residual = ExpressionTree.Empty();

      // If OR or Negate exists at this node, we only push down if the whole node is fully contained.
      if (!safeFilter.MatchAll || safeFilter.Negate) {
        RouteWholeTreeOrResidual(safeFilter, routing, primary, secondary, residual);
        return BuildResult(primary, secondary, residual);
      }

      // MatchAll == true and Negate == false: split safely,
      // but remember that duplicate predicates on same field behave like OR per field.
      // Therefore we MUST keep same-field predicate groups intact.
      Dictionary<string, List<FieldPredicate>> grouped = GroupPredicatesByField(safeFilter);

      foreach (KeyValuePair<string, List<FieldPredicate>> kvp in grouped) {
        string fieldName = kvp.Key;
        List<FieldPredicate> group = kvp.Value;

        PredicateTarget groupTarget = ResolveGroupTarget(group, routing);

        if (groupTarget == PredicateTarget.Primary) {
          AppendPredicateGroup(primary, group);
        }
        else if (groupTarget == PredicateTarget.Secondary) {
          AppendPredicateGroup(secondary, group);
        }
        else {
          AppendPredicateGroup(residual, group);
        }
      }

      // Split subtrees recursively.
      if (safeFilter.SubTree != null && safeFilter.SubTree.Count > 0) {
        int i = 0;
        while (i < safeFilter.SubTree.Count) {
          ExpressionTree sub = safeFilter.SubTree[i];

          ExpressionTreeSplitResult subSplit = Split(sub, routing);

          AppendIfNotEmpty(primary, subSplit.PrimaryPushdown);
          AppendIfNotEmpty(secondary, subSplit.SecondaryPushdown);
          AppendIfNotEmpty(residual, subSplit.Residual);

          i++;
        }
      }

      return BuildResult(primary, secondary, residual);
    }

    private static void RouteWholeTreeOrResidual(
      ExpressionTree tree,
      PredicateRoutingMap routing,
      ExpressionTree primary,
      ExpressionTree secondary,
      ExpressionTree residual) {
      PredicateTarget wholeTarget = routing.ResolveTree(tree);

      if (wholeTarget == PredicateTarget.Primary) {
        AppendIfNotEmpty(primary, tree);
      }
      else if (wholeTarget == PredicateTarget.Secondary) {
        AppendIfNotEmpty(secondary, tree);
      }
      else {
        AppendIfNotEmpty(residual, tree);
      }
    }

    private static Dictionary<string, List<FieldPredicate>> GroupPredicatesByField(ExpressionTree filter) {
      Dictionary<string, List<FieldPredicate>> grouped = new Dictionary<string, List<FieldPredicate>>(StringComparer.OrdinalIgnoreCase);

      if (filter.Predicates == null || filter.Predicates.Count == 0) {
        return grouped;
      }

      int i = 0;
      while (i < filter.Predicates.Count) {
        FieldPredicate predicate = filter.Predicates[i];
        string key = predicate.FieldName;
        if (string.IsNullOrEmpty(key)) {
          key = string.Empty;
        }

        List<FieldPredicate> list;
        if (!grouped.TryGetValue(key, out list)) {
          list = new List<FieldPredicate>();
          grouped[key] = list;
        }

        list.Add(predicate);
        i++;
      }

      return grouped;
    }

    private static PredicateTarget ResolveGroupTarget(List<FieldPredicate> group, PredicateRoutingMap routing) {
      PredicateTarget overall = PredicateTarget.Unknown;

      int i = 0;
      while (i < group.Count) {
        FieldPredicate p = group[i];
        PredicateTarget target = routing.Resolve(p.FieldName);

        if (target == PredicateTarget.Unknown) {
          return PredicateTarget.Unknown;
        }

        if (overall == PredicateTarget.Unknown) {
          overall = target;
        }
        else if (overall != target) {
          return PredicateTarget.Unknown;
        }

        i++;
      }

      return overall;
    }

    private static void AppendPredicateGroup(ExpressionTree target, List<FieldPredicate> group) {
      int i = 0;
      while (i < group.Count) {
        target.Predicates.Add(group[i]);
        i++;
      }
    }

    private static void AppendIfNotEmpty(ExpressionTree target, ExpressionTree source) {
      if (source == null) {
        return;
      }

      if (IsEmpty(source)) {
        return;
      }

      if (target.SubTree == null) {
        target.SubTree = new List<ExpressionTree>();
      }

      target.SubTree.Add(source);
    }

    private static ExpressionTreeSplitResult BuildResult(ExpressionTree primary, ExpressionTree secondary, ExpressionTree residual) {
      ExpressionTreeSplitResult result = new ExpressionTreeSplitResult();
      result.PrimaryPushdown = IsEmpty(primary) ? null : primary;
      result.SecondaryPushdown = IsEmpty(secondary) ? null : secondary;
      result.Residual = IsEmpty(residual) ? null : residual;
      return result;
    }

    private static bool IsEmpty(ExpressionTree tree) {
      if (tree == null) {
        return true;
      }

      if (tree.Predicates != null && tree.Predicates.Count > 0) {
        return false;
      }

      if (tree.SubTree != null && tree.SubTree.Count > 0) {
        return false;
      }

      return true;
    }

  }
 
}
