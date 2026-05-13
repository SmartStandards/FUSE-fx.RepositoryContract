using System;
using System.Diagnostics;
#if !NETCOREAPP
using System.Globalization;
using System.Text;
#endif
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

    public string ValueSerialized { get; set; } = null;

#if NETCOREAPP
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
        return string.Empty;
      }
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

#if !NETCOREAPP
    public static string SerializeValueForNetFramework(object value) {
      if (value == null) {
        return "null";
      }
      if (value is string stringValue) {
        return $"\"{EscapeJsonString(stringValue)}\"";
      }
      if (value is char charValue) {
        return $"\"{EscapeJsonString(charValue.ToString())}\"";
      }
      if (value is bool boolValue) {
        return boolValue ? "true" : "false";
      }
      if (value is DateTime dateTimeValue) {
        return $"\"{dateTimeValue:O}\"";
      }
      if (value is DateTimeOffset dateTimeOffsetValue) {
        return $"\"{dateTimeOffsetValue:O}\"";
      }
      if (value is Guid guidValue) {
        return $"\"{guidValue:D}\"";
      }
      if (value is byte[] bytesValue) {
        return $"\"{Convert.ToBase64String(bytesValue)}\"";
      }
      if (value is Enum) {
        return Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
      }
      if (value is IFormattable formattableValue) {
        return formattableValue.ToString(null, CultureInfo.InvariantCulture);
      }
      return $"\"{EscapeJsonString(value.ToString())}\"";
    }

    private static string EscapeJsonString(string value) {
      StringBuilder builder = new StringBuilder(value.Length);
      foreach (char character in value) {
        switch (character) {
          case '\\':
            builder.Append("\\\\");
            break;
          case '"':
            builder.Append("\\\"");
            break;
          case '\b':
            builder.Append("\\b");
            break;
          case '\f':
            builder.Append("\\f");
            break;
          case '\n':
            builder.Append("\\n");
            break;
          case '\r':
            builder.Append("\\r");
            break;
          case '\t':
            builder.Append("\\t");
            break;
          default:
            if (char.IsControl(character)) {
              builder.Append($"\\u{(int)character:x4}");
            } else {
              builder.Append(character);
            }
            break;
        }
      }
      return builder.ToString();
    }
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
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate LessOrEqual(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.LessOrEqual,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
#endif
        Value = value
      };
    }

    public static FieldPredicate Less(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.Less,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
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
#else
        ValueSerialized = SerializeValueForNetFramework(value),
#endif
        Value = value
      };
    }

  }

}
