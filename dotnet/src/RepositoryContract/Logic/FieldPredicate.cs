using System;
using System.Collections.Generic;
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
  [DebuggerDisplay("{FieldName} {Operator} {ValueSerialized}")]
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
        return default;
      }
      return System.Text.Json.JsonSerializer.Deserialize<T>(ValueSerialized);
    }

    public string GetValueAsString() {
      return ValueSerialized ?? string.Empty;
    }

    public string GetValueAsRawString() {
      if (string.IsNullOrEmpty(ValueSerialized) || ValueSerialized == "null") return "";
      if (ValueSerialized.Length >= 2 && ValueSerialized[0] == '"' && ValueSerialized[ValueSerialized.Length - 1] == '"') {
        return System.Text.Json.JsonSerializer.Deserialize<string>(ValueSerialized) ?? "";
      }
      return ValueSerialized;
    }

    public void SetValue<T>(T value) {
      ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value);
    }
#endif

#if !NETCOREAPP
    public string GetValueAsRawString() {
      if (string.IsNullOrEmpty(ValueSerialized) || ValueSerialized == "null") return "";
      if (ValueSerialized.Length >= 2 && ValueSerialized[0] == '"' && ValueSerialized[ValueSerialized.Length - 1] == '"') {
        return UnescapeJsonString(ValueSerialized.Substring(1, ValueSerialized.Length - 2));
      }
      return ValueSerialized;
    }

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
      if (value is System.Collections.IEnumerable enumerableValue) {
        return SerializeArrayForNetFramework(enumerableValue);
      }
      if (value is Enum) {
        return Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
      }
      if (value is IFormattable formattableValue) {
        return formattableValue.ToString(null, CultureInfo.InvariantCulture);
      }
      return $"\"{EscapeJsonString(value.ToString())}\"";
    }

    public static string SerializeArrayForNetFramework(object arrayValue) {
      System.Collections.IEnumerable enumerable = arrayValue as System.Collections.IEnumerable;
      if (enumerable == null) {
        return "null";
      }
      StringBuilder sb = new StringBuilder("[");
      bool first = true;
      foreach (object item in enumerable) {
        if (!first) sb.Append(",");
        sb.Append(SerializeValueForNetFramework(item));
        first = false;
      }
      sb.Append("]");
      return sb.ToString();
    }

    public static object DeserializeValueForNetFramework(string json, Type targetType) {
      if (json == null || json == "null" || json.Length == 0) {
        if (targetType.IsValueType) {
          return Activator.CreateInstance(targetType);
        }
        return null;
      }

      Type underlyingNullable = Nullable.GetUnderlyingType(targetType);
      if (underlyingNullable != null) {
        return DeserializeValueForNetFramework(json, underlyingNullable);
      }

      if (targetType == typeof(bool)) {
        return json == "true";
      }

      if (json.Length >= 2 && json[0] == '"' && json[json.Length - 1] == '"') {
        string unescaped = UnescapeJsonString(json.Substring(1, json.Length - 2));
        if (targetType == typeof(string)) return unescaped;
        if (targetType == typeof(char)) return unescaped.Length > 0 ? unescaped[0] : '\0';
        if (targetType == typeof(DateTime)) return DateTime.ParseExact(unescaped, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType == typeof(DateTimeOffset)) return DateTimeOffset.ParseExact(unescaped, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType == typeof(Guid)) return Guid.Parse(unescaped);
        if (targetType == typeof(byte[])) return Convert.FromBase64String(unescaped);
        return unescaped;
      }

      if (targetType.IsEnum) {
        long enumValue = long.Parse(json, CultureInfo.InvariantCulture);
        return Enum.ToObject(targetType, enumValue);
      }

      if (typeof(IConvertible).IsAssignableFrom(targetType)) {
        return Convert.ChangeType(json, targetType, CultureInfo.InvariantCulture);
      }

      throw new NotSupportedException($"Cannot deserialize JSON '{json}' to type {targetType.FullName}");
    }

    public static object[] DeserializeArrayForNetFramework(string json, Type elementType) {
      List<string> parts = GetJsonArrayElementJsons(json);
      object[] result = new object[parts.Count];
      for (int i = 0; i < parts.Count; i++) {
        result[i] = DeserializeValueForNetFramework(parts[i].Trim(), elementType);
      }
      return result;
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

    private static string UnescapeJsonString(string value) {
      if (!value.Contains("\\")) return value;
      StringBuilder sb = new StringBuilder(value.Length);
      for (int i = 0; i < value.Length; i++) {
        if (value[i] == '\\' && i + 1 < value.Length) {
          i++;
          switch (value[i]) {
            case '"': sb.Append('"'); break;
            case '\\': sb.Append('\\'); break;
            case 'b': sb.Append('\b'); break;
            case 'f': sb.Append('\f'); break;
            case 'n': sb.Append('\n'); break;
            case 'r': sb.Append('\r'); break;
            case 't': sb.Append('\t'); break;
            case 'u':
              if (i + 4 < value.Length) {
                string hex = value.Substring(i + 1, 4);
                sb.Append((char)Convert.ToInt32(hex, 16));
                i += 4;
              }
              break;
            default: sb.Append(value[i]); break;
          }
        } else {
          sb.Append(value[i]);
        }
      }
      return sb.ToString();
    }
#endif

    /// <summary>
    /// Splits a JSON array string into the raw JSON representations of its elements.
    /// E.g. "[\"DE\",\"AT\",1]" → ["\"DE\"", "\"AT\"", "1"]
    /// </summary>
    public static List<string> GetJsonArrayElementJsons(string json) {
      List<string> empty = new List<string>();
      if (string.IsNullOrEmpty(json) || json == "null") return empty;
      json = json.Trim();
      if (json.Length < 2 || json[0] != '[' || json[json.Length - 1] != ']') return empty;
      string inner = json.Substring(1, json.Length - 2).Trim();
      if (inner.Length == 0) return empty;
      return SplitJsonArray(inner);
    }

    private static List<string> SplitJsonArray(string inner) {
      List<string> parts = new List<string>();
      int depth = 0;
      bool inString = false;
      bool escaped = false;
      int start = 0;

      for (int i = 0; i < inner.Length; i++) {
        char c = inner[i];
        if (escaped) { escaped = false; continue; }
        if (c == '\\') { escaped = true; continue; }
        if (c == '"') { inString = !inString; continue; }
        if (inString) continue;
        if (c == '{' || c == '[') depth++;
        else if (c == '}' || c == ']') depth--;
        else if (c == ',' && depth == 0) {
          parts.Add(inner.Substring(start, i - start));
          start = i + 1;
        }
      }
      parts.Add(inner.Substring(start));
      return parts;
    }

    public override string ToString() {
      return $"{FieldName} {Operator} {ValueSerialized}";
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
      };
    }

    public static FieldPredicate EndsWith(string fieldName, object value) {
      return new FieldPredicate() {
        FieldName = fieldName,
        Operator = FieldOperators.EndsWith,
#if NETCOREAPP
        ValueSerialized = System.Text.Json.JsonSerializer.Serialize(value),
#else
        ValueSerialized = SerializeValueForNetFramework(value),
#endif
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
      };
    }

  }

}
