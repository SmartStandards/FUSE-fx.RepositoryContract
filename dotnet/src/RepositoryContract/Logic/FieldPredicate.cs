using System;
using System.Diagnostics;
#if NETCOREAPP
using System.Text.Json;
#endif

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
    public string ValueSerialized { get; set; } = null;

    public T? TryGetValue<T>() {
      if (string.IsNullOrEmpty(ValueSerialized)) {
        if (typeof(JsonElement).IsAssignableFrom(Value.GetType())) {
          return ((JsonElement)Value).Deserialize<T>();
        }
        return (T)Value;
      }
      return System.Text.Json.JsonSerializer.Deserialize<T>(ValueSerialized);
    }

    public string GetValueAsString() {
      if (string.IsNullOrEmpty(ValueSerialized)) {
        if (typeof(JsonElement).IsAssignableFrom(Value.GetType())) {
          return ((JsonElement)Value).ToString();
        }
        return Value.ToString();
      }
      return ValueSerialized;
    }

    public void SetValue<T>(T value) {
      ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value);
      Value = value;
    }
#endif

    /// <summary>
    /// The value to match!
    /// (NOTE: in th special case of using the 'in' operator,
    /// the given 'value' to match must NOT be scalar!
    /// Instead it must be an ARRAY. A match is given if a field equals to
    /// at least one value within that array.)
    /// </summary>
    public object Value { get; set; }

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
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value,
      };
    }

    public static FieldPredicate NotEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.NotEqual,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate GreaterOrEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.GreaterOrEqual,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate Greater(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Greater,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate StartsWith(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.StartsWith,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate SubstringOf(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.SubstringOf,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate Contains(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Contains,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#endif
        Value = value
      };
    }

  }

}
