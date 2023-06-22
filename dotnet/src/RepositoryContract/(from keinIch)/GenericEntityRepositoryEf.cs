using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace System.Data.Fuse {
  public class GenericEntityRepositoryEf : IGenericEntityRepository {
    private readonly DbContext _DbContext;
    private readonly Assembly _Assembly;

    public GenericEntityRepositoryEf(DbContext dbContext, Assembly assembly) {
      this._DbContext = dbContext;
      this._Assembly = assembly;
    }

    public void DeleteEntities(object[][] entityIdsToDelete) {
      throw new System.NotImplementedException();
    }

    public IList<Dictionary<string, object>> GetDtos(string entityName) {
      IList entities = GetEntities(entityName);
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }
      IEntityType efEntityType = _DbContext.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == entityType.FullName);

      IList<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

      foreach (object entity in entities) {
        Dictionary<string, object> dto = new Dictionary<string, object>();
        foreach (IProperty scalarProperty in efEntityType.GetProperties()) {
          if (scalarProperty.IsForeignKey()) { continue; }
          dto.Add(scalarProperty.Name, entityType.GetProperty(scalarProperty.Name).GetValue(entity));
        }
        foreach (INavigation navProp in efEntityType.GetNavigations()) {
          PropertyInfo navPropTarget = entityType.GetProperty(navProp.Name);
          if (navPropTarget == null) { continue; }
          object navPropValue = navPropTarget.GetValue(entity);
          if (navPropValue == null) {
            dto.Add(navProp.Name, null);
            continue;
          }
          PropertyInfo navIdProp = navPropValue.GetType().GetProperty("Id");
          if (navIdProp == null) { continue; }
          object navId = navIdProp.GetValue(navPropValue);
          EntityRefById entityRef = new EntityRefById();
          entityRef.Id = navId.ToString();
          entityRef.Label = navPropValue.ToString();
          dto.Add(navProp.Name, entityRef);
        }
        result.Add(dto);
      }
      return result;
    }

    public IList GetEntities(string entityName) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }
      MethodInfo dbSetMethod = typeof(DbContext).GetMethod("Set", new Type[] { });
      dbSetMethod = dbSetMethod.MakeGenericMethod(entityType);
      IQueryable resultSet = (IQueryable)dbSetMethod.Invoke(_DbContext, null);

      IEntityType efEntityType = _DbContext.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == entityType.FullName);
      foreach (var navProp in efEntityType.GetNavigations()) {
        IEnumerable<MethodInfo> includeMethods = typeof(EntityFrameworkQueryableExtensions).GetMethods().Where((mi) => mi.Name == "Include");
        MethodInfo includeMethod = includeMethods.FirstOrDefault(
          (mi) => mi.GetParameters().Length == 2 && mi.GetParameters()[1].ParameterType == typeof(string)
        );
        includeMethod = includeMethod.MakeGenericMethod(entityType);
        resultSet = (IQueryable)includeMethod.Invoke(null, new object[] { resultSet, navProp.Name });
      }
     
      MethodInfo toListMethod = typeof(Enumerable).GetMethod("ToList");
      toListMethod = toListMethod.MakeGenericMethod(entityType);

      IList result = (IList)toListMethod.Invoke(null, new object[] { resultSet });
      return result;
    }

    public object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> entity) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }

      //TODO -> id?
      if (!entity.ContainsKey("id")) { return null; }
      JsonElement id = entity["id"];
      Int32 idValue = id.GetInt32();

      object existingEntity = _DbContext.Find(entityType, idValue);

      if (existingEntity == null) {
        return Add(entityType, entity);
      } else {
        CopyProperties(entity, existingEntity);
        _DbContext.SaveChanges();
        return existingEntity;
      }
    }

    private object Add(Type entityType, Dictionary<string, JsonElement> entity) {
      object newEntity = Activator.CreateInstance(entityType);
      CopyProperties(entity, newEntity);
      _DbContext.Add(newEntity);
      _DbContext.SaveChanges();
      return newEntity;
    }

    private void CopyProperties(Dictionary<string, JsonElement> source, object target) {
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

    private void SetPropertyValue(PropertyInfo targetProperty, object target, JsonElement propertyValue) {
      if (targetProperty.PropertyType == typeof(string)) {
        targetProperty.SetValue(target, propertyValue.GetString());
      } else if (targetProperty.PropertyType == typeof(Int64)) {
        targetProperty.SetValue(target, propertyValue.GetInt64());
      } else if (targetProperty.PropertyType == typeof(bool)) {
        targetProperty.SetValue(target, propertyValue.GetBoolean());
      } else if (targetProperty.PropertyType == typeof(DateTime)) {
        targetProperty.SetValue(target, propertyValue.GetDateTime());
      }
    }

    public IList<EntityRefById> GetEntityRefs(string entityName) {
      IList entities = GetEntities(entityName);
      List<EntityRefById> result = new List<EntityRefById>();
      if (entities.Count == 0) { return result; }
      Type entityType = entities[0].GetType();
      PropertyInfo idProp = entityType.GetProperty("Id");
      if (idProp == null) { return result; }
      foreach (object entity in entities) {
        EntityRefById entityRef = new EntityRefById();
        entityRef.Id = idProp.GetValue(entity).ToString();
        entityRef.Label = entity.ToString();
        result.Add(entityRef);
      }
      return result;
    }
  }

  internal static class StringExtensions {
    public static string CapitalizeFirst(this string str) {
      return char.ToUpper(str[0]) + str.Substring(1);
    }
  }
}
