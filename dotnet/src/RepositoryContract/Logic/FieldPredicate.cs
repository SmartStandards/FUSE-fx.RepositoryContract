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

#if NETCOREAPP
    public string ValueSerialized { get; set; } = "null";

    public T? TryGetValue<T>() {
      return System.Text.Json.JsonSerializer.Deserialize<T>(ValueSerialized);
    }

    public void SetValue<T>(T value) {
      ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value);
    }
#else

    /// <summary>
    /// The value to match!
    /// (NOTE: in th special case of using the 'in' operator,
    /// the given 'value' to match must NOT be scalar!
    /// Instead it must be an ARRAY. A match is given if a field equals to
    /// at least one value within that array.)
    /// </summary>
    public object Value { get; set; }
#endif

    public override string ToString() {
#if NETCOREAPP
      return $"{FieldName} {Operator} {ValueSerialized}";
#else
      return $"{FieldName} {Operator} {Value}";
#endif
    }

    public static FieldPredicate Equal(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Equal,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate NotEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.NotEqual,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate GreaterOrEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.GreaterOrEqual,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate Greater(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Greater,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate StartsWith(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.StartsWith,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate SubstringOf(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.SubstringOf,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

    public static FieldPredicate Contains(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Contains,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value)
#else
        Value = value,
#endif
      };
    }

  }

}
