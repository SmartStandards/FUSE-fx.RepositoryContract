using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Text.Json;
#endif

namespace System.Data.Fuse.Convenience {

  public static class ConversionExtensions {

    public static IList<T2> ToBuseinssModels<T1, T2>(
      this IList<T1> entityList,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation,
      Action<T1, T2> onAfterConvert = null
    ) {
      return entityList.Select(entity => entity.ToBusinessModel(isForeignKey, isNavigation, onAfterConvert)).ToList();
    }

    public static IList<T2> ToEntities<T1, T2>(
      this IList<T1> businessModelList,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation,
      Action<T1, T2> onAfterConvert = null
    ) {
      return businessModelList.Select(entity => entity.ToEntity(isForeignKey, isNavigation, onAfterConvert)).ToList();
    }

    public static T2 ToBusinessModel<T1, T2>(
      this T1 entity,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation,
      Action<T1, T2> onAfterConvert = null
    ) {
      T2 result = entity.ConvertToBusinessModelDynamic(isForeignKey, isNavigation).Deserialize<T2>();
      onAfterConvert?.Invoke(entity, result);
      return result;
    }

    public static T2 ToEntity<T1, T2>(
      this T1 businessModel,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation,
      Action<T1, T2> onAfterConvert = null
    ) {
      Func<PropertyInfo, bool> ignore = (pi) => { return isForeignKey(pi) || isNavigation(pi); };
      T2 result = businessModel.Serialize(ignore).ConvertToEntityDynamic<T2>(isForeignKey, isNavigation);
      onAfterConvert?.Invoke(businessModel, result);
      return result;
    }

    public static Dictionary<string, object> ConvertToBusinessModelDynamic<T>(
      this T entity,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      Type t = typeof(T);
      Dictionary<string, object> result = new Dictionary<string, object>();
      foreach (PropertyInfo pi in t.GetProperties()) {
        if (!pi.CanWrite) continue;
        if (isForeignKey(pi)) continue;
        if (isNavigation(pi)) {
          HandleNavigationPropertyToBusinessModel(entity, result, pi);
        } else {
          result.Add(pi.Name, pi.GetValue(entity));
        }
      }
      return result;
    }

    public static T ConvertToEntityDynamic<T>(
      this Dictionary<string, object> businessModel,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      Type t = typeof(T);
      T result = (T)Activator.CreateInstance(t);

      foreach (PropertyInfo pi in t.GetProperties()) {
        if (!pi.CanWrite) continue;
        if (isForeignKey(pi)) continue;
        if (isNavigation(pi)) {
          HandleNavigationPropertyToEntity(businessModel, result, pi);
        } else {
          if (!businessModel.TryGetValue(pi.Name, out object propValue)) { continue; }
          if (propValue == null) { pi.SetValue(result, null); continue; }
          if (pi.PropertyType.IsAssignableFrom(propValue.GetType())) {
            pi.SetValue(result, propValue);
          }
        }
      }
      return result;
    }

    private static void HandleNavigationPropertyToEntity<T>(
      Dictionary<string, object> businessModel, T result, PropertyInfo pi
    ) {
      PropertyInfo foreignKeyProp = typeof(T).GetProperty(pi.Name + "Id");
      if (foreignKeyProp == null) { return; }
      if (!businessModel.TryGetValue(pi.Name, out object navPropValue)) { return; }
      if (navPropValue == null) { return; }
      if (!typeof(EntityRefById).IsAssignableFrom(navPropValue.GetType())) { return; }
      EntityRefById entityRef = (EntityRefById)navPropValue;
      if (foreignKeyProp.PropertyType == typeof(int)) {
        if (!int.TryParse(entityRef.Id, out int id)) { return; }
        foreignKeyProp.SetValue(result, id);
      }
      if (foreignKeyProp.PropertyType == typeof(long)) {
        if (!long.TryParse(entityRef.Id, out long id)) { return; }
        foreignKeyProp.SetValue(result, id);
      }
    }

    private static void HandleNavigationPropertyToBusinessModel<T>(
      T entity, Dictionary<string, object> result, PropertyInfo pi
    ) {
      object navPropValue = pi.GetValue(entity);
      if (navPropValue == null) {
        result.Add(pi.Name, null);
        return;
      }
      PropertyInfo navIdProp = navPropValue.GetType().GetProperty("Id");
      if (navIdProp == null) { return; }
      object navId = navIdProp.GetValue(navPropValue);
      EntityRefById entityRef = new EntityRefById();
      entityRef.Id = navId.ToString();
      entityRef.Label = navPropValue.ToString();
      result.Add(pi.Name, entityRef);
    }

    public static T Deserialize<T>(this Dictionary<string, object> businessModel) {
      Type type = typeof(T);
      T result = (T)Activator.CreateInstance(type);

      foreach (PropertyInfo pi in type.GetProperties()) {
        if (!pi.CanWrite) { continue; }
        if (!businessModel.TryGetValue(pi.Name, out object propValue)) {
          string propNameCap = pi.Name.Substring(0,1).ToLower() + pi.Name.Substring(1);
          if (!businessModel.TryGetValue(propNameCap, out  propValue)) { continue; }
        }
        if (propValue == null) { pi.SetValue(result, null); }
        if (pi.PropertyType.IsAssignableFrom(propValue.GetType())) {
          pi.SetValue(result, propValue, null);
        }
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(propValue.GetType())) {
          pi.SetValue(result, GetValue(pi, (JsonElement)propValue));
        }
#endif
      }
      return result;
    }

#if NETCOREAPP
    public static object GetValue(PropertyInfo prop, JsonElement propertyValue) {
      if (prop.PropertyType == typeof(string)) {
        return propertyValue.GetString();
      } else if (prop.PropertyType == typeof(Int64)) {
        return propertyValue.GetInt64();
      } else if (prop.PropertyType == typeof(bool)) {
        return propertyValue.GetBoolean();
      } else if (prop.PropertyType == typeof(DateTime)) {
        return propertyValue.GetDateTime();
      } else if (prop.PropertyType == typeof(Int32)) {
        return propertyValue.GetInt32();
      } else if (prop.PropertyType == typeof(decimal)) {
        return propertyValue.GetDecimal();
      } else if (prop.PropertyType == typeof(Guid)) {
        return propertyValue.GetGuid();
      } else if (prop.PropertyType == typeof(double)) {
        return propertyValue.GetDouble();
      } else {
        return null;
      }
    }
#endif

    public static Dictionary<string, object> Serialize<T>(this T input, Func<PropertyInfo, bool> ignore) {
      Type type = typeof(T);
      Dictionary<string, object> result = new Dictionary<string, object>();
      foreach (PropertyInfo pi in type.GetProperties()) {
        if (!pi.CanWrite) { continue; }
        if (ignore(pi)) { continue; }
        result.Add(pi.Name, pi.GetValue(input, null));
      }
      return result;
    }

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