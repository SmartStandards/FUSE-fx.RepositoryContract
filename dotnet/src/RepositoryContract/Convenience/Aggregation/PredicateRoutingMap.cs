using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Data.Fuse.Convenience.Internal;
using System.Data.ModelDescription;

namespace System.Data.Fuse.Convenience.Aggregation {

  /// <summary>
  /// Defines routing decisions for predicates and trees to push down into one of multiple sources.
  /// </summary>
  public sealed class PredicateRoutingMap {

    private readonly HashSet<string> _PrimaryFieldNames;
    private readonly HashSet<string> _SecondaryFieldNames;

    /// <summary>
    /// Initializes a new routing map with field-name ownership.
    /// </summary>
    public PredicateRoutingMap(string[] primaryFieldNames, string[] secondaryFieldNames) {
      if (primaryFieldNames == null) {
        throw new ArgumentNullException(nameof(primaryFieldNames));
      }
      if (secondaryFieldNames == null) {
        throw new ArgumentNullException(nameof(secondaryFieldNames));
      }

      _PrimaryFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _SecondaryFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      int i = 0;
      while (i < primaryFieldNames.Length) {
        if (!string.IsNullOrEmpty(primaryFieldNames[i])) {
          _PrimaryFieldNames.Add(primaryFieldNames[i]);
        }
        i++;
      }

      int s = 0;
      while (s < secondaryFieldNames.Length) {
        if (!string.IsNullOrEmpty(secondaryFieldNames[s])) {
          _SecondaryFieldNames.Add(secondaryFieldNames[s]);
        }
        s++;
      }
    }

    /// <summary>
    /// Resolves a single field name to a predicate target.
    /// </summary>
    public PredicateTarget Resolve(string fieldName) {
      if (string.IsNullOrEmpty(fieldName)) {
        return PredicateTarget.Unknown;
      }

      if (_PrimaryFieldNames.Contains(fieldName)) {
        return PredicateTarget.Primary;
      }

      if (_SecondaryFieldNames.Contains(fieldName)) {
        return PredicateTarget.Secondary;
      }

      return PredicateTarget.Unknown;
    }

    /// <summary>
    /// Resolves whether an entire ExpressionTree is fully contained in one source (Primary or Secondary).
    /// If mixed or unknown fields exist, returns Unknown.
    /// </summary>
    public PredicateTarget ResolveTree(ExpressionTree tree) {
      if (tree == null) {
        return PredicateTarget.Unknown;
      }

      PredicateTarget overall = PredicateTarget.Unknown;

      if (tree.Predicates != null && tree.Predicates.Count > 0) {
        int i = 0;
        while (i < tree.Predicates.Count) {
          FieldPredicate predicate = tree.Predicates[i];
          PredicateTarget target = this.Resolve(predicate.FieldName);
          overall = MergeOverall(overall, target);
          if (overall == PredicateTarget.Unknown) {
            return PredicateTarget.Unknown;
          }
          i++;
        }
      }

      if (tree.SubTree != null && tree.SubTree.Count > 0) {
        int s = 0;
        while (s < tree.SubTree.Count) {
          PredicateTarget subTarget = this.ResolveTree(tree.SubTree[s]);
          overall = MergeOverall(overall, subTarget);
          if (overall == PredicateTarget.Unknown) {
            return PredicateTarget.Unknown;
          }
          s++;
        }
      }

      return overall;
    }

    private static PredicateTarget MergeOverall(PredicateTarget overall, PredicateTarget next) {
      if (next == PredicateTarget.Unknown) {
        return PredicateTarget.Unknown;
      }

      if (overall == PredicateTarget.Unknown) {
        return next;
      }

      if (overall == next) {
        return overall;
      }

      return PredicateTarget.Unknown;
    }

  }

  /// <summary>
  /// Target repository for predicate pushdown decisions.
  /// </summary>
  public enum PredicateTarget {
    Unknown = 0,
    Primary = 1,
    Secondary = 2
  }

}
