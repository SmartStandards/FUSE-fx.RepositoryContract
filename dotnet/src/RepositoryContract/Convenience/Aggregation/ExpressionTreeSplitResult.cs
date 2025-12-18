using System;

namespace System.Data.Fuse.Convenience.Aggregation {

  /// <summary>
  /// Result of splitting a filter into pushdown parts and a residual part.
  /// The intended recomposition is:
  ///   Original ≡ (PrimaryPushdown AND SecondaryPushdown AND Residual)
  /// </summary>
  public sealed class ExpressionTreeSplitResult {

    public ExpressionTreeSplitResult() {
    }

    public ExpressionTree PrimaryPushdown { get; set; }

    public ExpressionTree SecondaryPushdown { get; set; }

    public ExpressionTree Residual { get; set; }

  }

}
