using System.Collections.Generic;
using System.Reflection;
#if NETCOREAPP
using System.Text.Json;
#endif

namespace System.Data.Fuse {

  public static class Utils {

    public static void CopyProperties(Dictionary<string, object> source, object target) {
      foreach (KeyValuePair<string, object> sourceProperty in source) {
        object propertyValue = sourceProperty.Value;
        if (propertyValue == null) {
          PropertyInfo targetProperty = target.GetType().GetProperty(sourceProperty.Key.CapitalizeFirst());
          targetProperty.SetValue(target, null);
          continue;
        }
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(propertyValue.GetType())) {
          JsonElement propertyValueJson = (JsonElement)propertyValue;
          if (propertyValueJson.ValueKind == JsonValueKind.Object) {
            JsonElement foreignKeyProperty;
            if (!propertyValueJson.TryGetProperty("id", out foreignKeyProperty)) { continue; }
            Int32 idValue = foreignKeyProperty.GetInt32();
            string foreignKeyPropertyName = sourceProperty.Key + "Id";
            PropertyInfo foreignKeyPropertyTarget = target.GetType().GetProperty(foreignKeyPropertyName.CapitalizeFirst());
            if (foreignKeyPropertyTarget == null) { continue; }
            foreignKeyPropertyTarget.SetValue(target, idValue);
          } else {
            PropertyInfo targetProperty = target.GetType().GetProperty(sourceProperty.Key.CapitalizeFirst());
            if (targetProperty == null) { continue; }
            SetPropertyValue(targetProperty, target, propertyValueJson);
          }
        } else {
#endif
        if (propertyValue.GetType().IsClass) {
          PropertyInfo otherIdProperty = propertyValue.GetType().GetProperty("Id");
          if (otherIdProperty == null) { continue; }
          string foreignKeyPropertyName = sourceProperty.Key + "Id";
          PropertyInfo foreignKeyPropertyTarget = target.GetType().GetProperty(foreignKeyPropertyName.CapitalizeFirst());
          if (foreignKeyPropertyTarget == null) { continue; }
          object idValue = otherIdProperty.GetValue(propertyValue);
          foreignKeyPropertyTarget.SetValue(target, idValue);
#if NETCOREAPP
          }
#endif
        }
      }
    }

#if NETCOREAPP
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
#endif
  }

}