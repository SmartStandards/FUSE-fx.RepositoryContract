using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class ExpressionTree {

    /// <summary>
    /// true: AND-Relation | false: OR-Relation
    /// </summary>
    public bool MatchAll { get; set; } = true;

    /// <summary>
    /// Negates the result
    /// </summary>
    public bool Negate { get; set; } = false;

    /// <summary>
    /// Can contain ATOMIC predicates (FieldName~Value).
    /// NOTE: If there is more than one predicate with the same FieldName in combination with
    /// MatchAll=true, then this will lead to an subordinated OR-Expression dedicated to this field.
    /// </summary>
    public List<FieldPredicate> Predicates { get; set; } = new List<FieldPredicate>();

    public List<ExpressionTree> SubTree { get; set; } = null;

    #region " ToString() - for DEBUGGING "

    public override string ToString() {
      var sb = new StringBuilder(500);
      int count = 0;

      foreach (string fieldName in this.Predicates.Select((p) => p.FieldName).Distinct()) {
        var pds = this.Predicates.Where((p) => p.FieldName == fieldName).Select((p) => p.ToString()).ToArray();
        string consolidated;
        if (pds.Length == 0) {
          consolidated = pds[0];
        }
        else {
          consolidated = "( " + string.Join(" || ", pds) + " )";
        }
        if (count > 1) {
          if (this.MatchAll) {
            sb.Append(" && ");
          }
          else {
            sb.Append(" || ");
          }
        }
        sb.Append(consolidated);
        count++;
      }

      foreach (ExpressionTree expre in this.SubTree) {
        if (count > 1) {
          if (this.MatchAll) {
            sb.Append(" && ");
          }
          else {
            sb.Append(" || ");
          }
        }
        sb.Append(expre.ToString());
        count++;
      }

      if(count > 1) {
        sb.Append( " )");
        sb.Insert(0,"( ");
      }

      return sb.ToString();
    }

    #endregion

    //AI after typing "public static ExpressionTree"
    public static ExpressionTree And(params FieldPredicate[] predicates) {
      return new ExpressionTree() {
        MatchAll = true,
        Predicates = predicates.ToList()
      };
    }

  }

}
