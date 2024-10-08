using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Data.Fuse.Convenience.Internal;
#if NETCOREAPP
using System.Text.Json;
using System.Threading;
#endif

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public static class ConversionExtensions {

    public static IList<T2> ToBuseinssModels<T1, T2>(
      this IList<T1> entityList,
      Func<PropertyInfo, T1, Dictionary<string, object>, bool> handleProperty,
      Action<T1, T2> onAfterConvert = null
    ) {
      return entityList.Select(entity => entity.ToBusinessModel(handleProperty, onAfterConvert)).ToList();
    }

    public static IList<Dictionary<string, object>> ToBusinessModelsDynamic<T1>(
      this IList<T1> entityList,
      Func<PropertyInfo, T1, Dictionary<string, object>, bool> handleProperty
    ) {
      return entityList.Select(entity => entity.ConvertToBusinessModelDynamic(handleProperty)).ToList();
    }

    public static IList<T2> ToEntities<T1, T2>(
      this IList<T1> businessModelList,
      Func<PropertyInfo, Dictionary<string, object>, T2, bool> handleProperty,
      Action<T1, T2> onAfterConvert = null
    ) {
      return businessModelList.Select(entity => entity.ToEntity(handleProperty, onAfterConvert)).ToList();
    }

    public static IList<T1> ToEntitiesDynamic<T1>(
      this IList<Dictionary<string, object>> businessModelList,
      Func<PropertyInfo, Dictionary<string, object>, T1, bool> handleProperty
    ) {
      return businessModelList.Select(bm => bm.ConvertToEntityDynamic<T1>(handleProperty)).ToList();
    }

    public static object ToBusinessModel(
     this object entity,
     Type sourceType, Type targetType,
     Func<PropertyInfo, object, Dictionary<string, object>, bool> handleProperty
   ) {
      object result = entity.ConvertToBusinessModelDynamic(handleProperty).Deserialize(targetType);
      return result;
    }

    public static T2 ToBusinessModel<T1, T2>(
      this T1 entity,
      Func<PropertyInfo, T1, Dictionary<string, object>, bool> handleProperty,
      Action<T1, T2> onAfterConvert = null
    ) {
      T2 result = entity.ConvertToBusinessModelDynamic(handleProperty).Deserialize<T2>();
      onAfterConvert?.Invoke(entity, result);
      return result;
    }

    public static T2 ToEntity<T1, T2>(
      this T1 businessModel,
      Func<PropertyInfo, Dictionary<string, object>, T2, bool> handleProperty,
      Action<T1, T2> onAfterConvert = null
    ) {
      T2 result = businessModel.Serialize().ConvertToEntityDynamic<T2>(handleProperty);
      onAfterConvert?.Invoke(businessModel, result);
      return result;
    }

    public static Dictionary<string, object> ConvertToBusinessModelDynamic<T>(
      this T entity,
      Func<PropertyInfo, T, Dictionary<string, object>, bool> handleProperty
    ) {
      Type t = typeof(T);
      Dictionary<string, object> result = new Dictionary<string, object>();
      bool initVisitedTypeNames = ConversionHelper._VisitedTypeNames == null;
      if (initVisitedTypeNames) {
        ConversionHelper._VisitedTypeNames = new AsyncLocal<List<string>>();
        ConversionHelper._VisitedTypeNames.Value = new List<string>();
      }
      foreach (PropertyInfo pi in t.GetProperties()) {
        if (!pi.CanWrite) continue;
        if (handleProperty(pi, entity, result)) continue;
        if (result.ContainsKey(pi.Name)) { continue; }
        result.Add(pi.Name, pi.GetValue(entity));
      }
      if (initVisitedTypeNames) {
        ConversionHelper._VisitedTypeNames = null;
      }
      return result;
    }

    public static T ConvertToEntityDynamic<T>(
      this Dictionary<string, object> businessModel,
      Func<PropertyInfo, Dictionary<string, object>, T, bool> handleProperty
    ) {
      Type t = typeof(T);
      T result = (T)Activator.CreateInstance(t);

      foreach (PropertyInfo pi in t.GetProperties()) {
        if (!pi.CanWrite) continue;
        if (handleProperty(pi, businessModel, result)) continue;

        if (!businessModel.TryGetValue(pi.Name, out object propValue)) { continue; }
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(propValue.GetType())) {
          propValue = ConversionHelper.GetValueFromJsonElementByType((JsonElement)propValue, pi.PropertyType);
        }
#endif
        if (propValue == null) { pi.SetValue(result, null); continue; }
        if (pi.PropertyType.IsAssignableFrom(propValue.GetType())) {
          pi.SetValue(result, propValue);
        }

      }
      return result;
    }

    //private static void HandleNavigationPropertyToEntity<T>(
    //  Dictionary<string, object> businessModel, T result, PropertyInfo pi
    //) {
    //  PropertyInfo foreignKeyProp = typeof(T).GetProperty(pi.Name + "Id");
    //  if (foreignKeyProp == null) { return; }
    //  if (!businessModel.TryGetValue(pi.Name, out object navPropValue)) { return; }
    //  if (navPropValue == null) { return; }
    //  if (!typeof(EntityRefById).IsAssignableFrom(navPropValue.GetType())) { return; }
    //  EntityRefById entityRef = (EntityRefById)navPropValue;
    //  if (foreignKeyProp.PropertyType == typeof(int)) {
    //    if (!int.TryParse(entityRef.Id, out int id)) { return; }
    //    foreignKeyProp.SetValue(result, id);
    //  }
    //  if (foreignKeyProp.PropertyType == typeof(long)) {
    //    if (!long.TryParse(entityRef.Id, out long id)) { return; }
    //    foreignKeyProp.SetValue(result, id);
    //  }
    //}

    //private static void HandleNavigationPropertyToBusinessModel<T>(
    //  T entity, Dictionary<string, object> result, PropertyInfo pi
    //) {
    //  object navPropValue = pi.GetValue(entity);
    //  if (navPropValue == null) {
    //    result.Add(pi.Name, null);
    //    return;
    //  }
    //  PropertyInfo navIdProp = navPropValue.GetType().GetProperty("Id");
    //  if (navIdProp == null) { return; }
    //  object navId = navIdProp.GetValue(navPropValue);
    //  EntityRefById entityRef = new EntityRefById();
    //  entityRef.Id = navId.ToString();
    //  entityRef.Label = navPropValue.ToString();
    //  result.Add(pi.Name, entityRef);
    //}

    public static T Deserialize<T>(this Dictionary<string, object> businessModel ) {
      Type type = typeof(T);
      return (T)Deserialize(businessModel, type);      
    }

    public static object Deserialize(this Dictionary<string, object> businessModel, Type type) {
      object result = Activator.CreateInstance(type);

      foreach (PropertyInfo pi in type.GetProperties()) {
        if (!pi.CanWrite) { continue; }
        if (!businessModel.TryGetValue(pi.Name, out object propValue)) {
          string propNameCap = pi.Name.Substring(0, 1).ToLower() + pi.Name.Substring(1);
          if (!businessModel.TryGetValue(propNameCap, out propValue)) { continue; }
        }
        if (propValue == null) { pi.SetValue(result, null); continue; }
        if (pi.PropertyType.IsAssignableFrom(propValue.GetType())) {
          pi.SetValue(result, propValue, null);
        }
        if (
          typeof(IEnumerable).IsAssignableFrom(propValue.GetType()) &&
          !(propValue.GetType() == typeof(string))
        ) {
          if (typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)) {
            Type listType = typeof(List<>).MakeGenericType(pi.PropertyType.GetGenericArguments()[0]);
            MethodInfo toListMethod = typeof(Enumerable).GetMethod(
              "ToList"
            ).MakeGenericMethod(pi.PropertyType.GetGenericArguments()[0]);
            object list = toListMethod.Invoke(null, new object[] { propValue });
            if (pi.PropertyType.IsAssignableFrom(listType)) {
              pi.SetValue(result, list, null);
            }
          }
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

    public static Dictionary<string, object> Serialize<T>(this T input) {
      Type type = typeof(T);
      Dictionary<string, object> result = new Dictionary<string, object>();
      foreach (PropertyInfo pi in type.GetProperties()) {
        if (!pi.CanWrite) { continue; }
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