﻿using System.Collections.Generic;

namespace System.Data.Fuse.Logic {
  public class LogicalExpression {

    /// <summary>
    /// and, or, not
    /// </summary>
    public string Operator { get; set; }

    public List<LogicalExpression> ExpressionArguments { get; set; } = new List<LogicalExpression>();

    public IList<RelationElement> AtomArguments { get; set; } = new List<RelationElement>();
  }
}