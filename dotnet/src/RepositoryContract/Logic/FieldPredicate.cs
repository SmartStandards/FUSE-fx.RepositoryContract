using System;
using System.Diagnostics;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  [DebuggerDisplay("{FieldName} {Operator} {Value}")]
  public class FieldPredicate {

    //TODO: überlegen, ob als FieldName
    //  ein * für "VOLLTEXTSUCHE" (in dafür geeigeneten feldern gem sematik 'Content' im EntityModel)
    //  ein # für "ID-Suche"      (in dafür geeigeneten feldern gem sematik 'IdentityRepresenting' im EntityModel)
    //erlaubt sein könnte???

    public string FieldName { get; set; }

    /// <summary>
    /// Wellknown operators like '==' '!='
    /// (see 'FieldOperators'-Contants).
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// The value to match!
    /// (NOTE: in th special case of using the 'in' operator,
    /// the given 'value' to match must NOT be scalar!
    /// Instead it must be an ARRAY. A match is given if a field equals to
    /// at least one value within that array.)
    /// </summary>
    public object Value { get; set; }

    public override string ToString() {
      return $"{FieldName} {Operator} {Value}";
    }

    public static FieldPredicate Equal(string fieldName,  object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Equal,
        Value = value
      };
    }

    public static FieldPredicate NotEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.NotEqual,
        Value = value
      };
    }

    public static FieldPredicate GreaterOrEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.GreaterOrEqual,
        Value = value
      };
    }

    public static FieldPredicate Greater(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Greater,
        Value = value
      };
    }

    public static FieldPredicate StartsWith(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.StartsWith,
        Value = value
      };
    }

    public static FieldPredicate SubstringOf(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.SubstringOf,
        Value = value
      };
    }

    public static FieldPredicate Contains(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Contains,
        Value = value
      };
    }

  }

}
