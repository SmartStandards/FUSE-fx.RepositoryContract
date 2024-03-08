using System.Collections.Generic;

namespace System.Data.Fuse {

  public class ExpressionTree {

    /// <summary>
    /// true: AND-Relation | false: OR-Relation
    /// </summary>
    public bool MatchAll { get; set; } = true;

    public Dictionary<string,object> MatchValues { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// For fields, which are NOT present withing 'MatchNull', a null-value will result in a negative match.
    /// For fields which have 'MatchNull' configured as TRUE, a null-value will result in a positive match
    /// (this can be combined with having it also within the 'MatchValues' resulting in an OR relation).
    /// For fields which have configured it as FALSE, the matching will explicitely REQUIRE NULL.
    /// </summary>
    public Dictionary<string, bool> MatchNull { get; set; } = null;

    /// <summary>
    /// For details see the constants located in the 'ValueMatchBehaviour' constants
    /// ( NotEqual:0 | Equal:1 | Less:2 | LessOrEqual:3 | More:4 | MoreOrEqual:5 |
    ///   EndsWith:2 | SubstringOf:3 | StartsWith:4 | Contains:5 | ContainsNot:6).
    /// All fields, which are part of the 'MatchValues', but not present within 'MatchBehaviour'
    /// will be processed with the ValueMatchBehaviour 'Equal' (=1)
    /// </summary>
    public Dictionary<string, int> MatchBehaviour { get; set; } = null;

    public List<ExpressionTree> SubTree { get; set; } = null;

  }

}
