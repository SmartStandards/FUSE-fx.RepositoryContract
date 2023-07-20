using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace System.Data.Fuse {

  public static class Utils {

    public static void CopyProperties(Dictionary<string, JsonElement> source, object target) {
      foreach (KeyValuePair<string, JsonElement> sourceProperty in source) {
        JsonElement propertyValue = sourceProperty.Value;
        if (propertyValue.ValueKind == JsonValueKind.Object) {
          JsonElement foreignKeyProperty;
          if (!propertyValue.TryGetProperty("id", out foreignKeyProperty)) { continue; }
          Int32 idValue = foreignKeyProperty.GetInt32();
          string foreignKeyPropertyName = sourceProperty.Key + "Id";
          PropertyInfo foreignKeyPropertyTarget = target.GetType().GetProperty(foreignKeyPropertyName.CapitalizeFirst());
          if (foreignKeyPropertyTarget == null) { continue; }
          foreignKeyPropertyTarget.SetValue(target, idValue);
        } else {
          PropertyInfo targetProperty = target.GetType().GetProperty(sourceProperty.Key.CapitalizeFirst());
          if (targetProperty == null) { continue; }
          SetPropertyValue(targetProperty, target, propertyValue);
        }
      }
    }

    public static void SetPropertyValue(PropertyInfo targetProperty, object target, JsonElement propertyValue) {
      if (targetProperty.PropertyType == typeof(string)) {
        targetProperty.SetValue(target, propertyValue.GetString());
      } else if (targetProperty.PropertyType == typeof(Int64)) {
        targetProperty.SetValue(target, propertyValue.GetInt64());
      } else if (targetProperty.PropertyType == typeof(bool)) {
        targetProperty.SetValue(target, propertyValue.GetBoolean());
      } else if (targetProperty.PropertyType == typeof(DateTime)) {
        targetProperty.SetValue(target, propertyValue.GetDateTime());
      } else if (targetProperty.PropertyType == typeof(Int32)) {
        targetProperty.SetValue(target, propertyValue.GetInt32());
      }
    }

  }

}