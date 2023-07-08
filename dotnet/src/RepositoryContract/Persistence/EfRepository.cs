using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore.Metadata;

namespace System.Data.Fuse {

  public abstract class EfRepository {
    protected readonly DbContext context;

    public EfRepository(DbContext dbContext) {
      this.context = dbContext;
    }

    public abstract IList GetEntities1(SimpleExpressionTree filter);
    public abstract IList GetEntities1(string dynamicLinqFilter);

    public abstract object AddOrUpdate1(Dictionary<string, JsonElement> entity);

    public abstract IList<Dictionary<string, object>> GetDtos1(SimpleExpressionTree filter);
    public abstract IList<Dictionary<string, object>> GetDtos1(string dynamicLinqFilter);
  }

  public class EfRepository1<TEntity> : EfRepository, IRepository<TEntity> where TEntity : class, new() {

    public EfRepository1(DbContext context) : base(context) {
    }

    public override object AddOrUpdate1(Dictionary<string, JsonElement> entity) {


      IEntityType efEntityType = context.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == typeof(TEntity).FullName);
      IKey primaryKey = efEntityType.GetKeys().First((k) => k.IsPrimaryKey());
      List<object> keyValues = new List<object>();
      foreach (IProperty keyProp in primaryKey.Properties) {
        JsonElement keyPropValueJson = entity[keyProp.Name.ToLowerFirst()];
        object keyValue = GetValue(keyProp, keyPropValueJson);
        keyValues.Add(keyValue);
      }

      object existingEntity = context.Set<TEntity>().Find(keyValues.ToArray());

      if (existingEntity == null) {
        return Add(entity);
      } else {
        CopyProperties(entity, existingEntity);
        context.SaveChanges();
        return existingEntity;
      }
    }

    private object Add(Dictionary<string, JsonElement> entity) {
      TEntity newEntity = new TEntity();
      CopyProperties(entity, newEntity);
      context.Set<TEntity>().Add(newEntity);
      context.SaveChanges();
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
      } else if (targetProperty.PropertyType == typeof(Int32)) {
        targetProperty.SetValue(target, propertyValue.GetInt32());
      }
    }

    public TEntity AddOrUpdateEntity(Dictionary<string, JsonElement> entity) {
      throw new NotImplementedException();
    }

    public void DeleteEntities(object[][] entityIdsToDelete) {
      throw new NotImplementedException();
    }

    public override IList<Dictionary<string, object>> GetDtos1(SimpleExpressionTree filter) {
      IList entities = GetEntities1(filter);
      return ConvertToDtos(entities);
    }

    public override IList<Dictionary<string, object>> GetDtos1(string dynamicLinqFilter) {
      IList entities = GetEntitiesDynamic(dynamicLinqFilter).ToList();
      return ConvertToDtos(entities);
    }

    private IList<Dictionary<string, object>> ConvertToDtos(IList entities) {
      Type entityType = typeof(TEntity);
      IEntityType efEntityType = context.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == entityType.FullName);

      IList<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

      foreach (object entity in entities) {
        Dictionary<string, object> dto = new Dictionary<string, object>();
        foreach (IProperty scalarProperty in efEntityType.GetProperties()) {
          if (scalarProperty.IsForeignKey()) { continue; }
          dto.Add(scalarProperty.Name.ToLowerFirst(), entityType.GetProperty(scalarProperty.Name).GetValue(entity));
        }
        foreach (INavigation navProp in efEntityType.GetNavigations()) {
          PropertyInfo navPropTarget = entityType.GetProperty(navProp.Name);
          if (navPropTarget == null) { continue; }
          object navPropValue = navPropTarget.GetValue(entity);
          if (navPropValue == null) {
            dto.Add(navProp.Name.ToLowerFirst(), null);
            continue;
          }
          PropertyInfo navIdProp = navPropValue.GetType().GetProperty("Id");
          if (navIdProp == null) { continue; }
          object navId = navIdProp.GetValue(navPropValue);
          EntityRefById entityRef = new EntityRefById();
          entityRef.Id = navId.ToString();
          entityRef.Label = navPropValue.ToString();
          dto.Add(navProp.Name.ToLowerFirst(), entityRef);
        }
        result.Add(dto);
      }
      return result;
    }

    public IList<Dictionary<string, object>> GetDtos() {
      throw new NotImplementedException();
    }

    public override IList GetEntities1(SimpleExpressionTree filter) {
      return GetEntitiesDynamic(filter.CompileToDynamicLinq()).ToList();
    }

    public override IList GetEntities1(string dynamicLinqFilter) {
      return GetEntitiesDynamic(dynamicLinqFilter).ToList();
    }

    public IList<EntityRefById> GetEntityRefs() {
      throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetEntitiesDynamic(string dynamicLinqFilter) {
      if (string.IsNullOrEmpty(dynamicLinqFilter)) {
        return context.Set<TEntity>();
      }
      return context.Set<TEntity>().Where(dynamicLinqFilter);
    }

    public IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> filter) {
      return context.Set<TEntity>().Where(filter);
    }
    private object GetValue(IProperty prop, JsonElement propertyValue) {
      if (prop.PropertyInfo.PropertyType == typeof(string)) {
        return propertyValue.GetString();
      } else if (prop.PropertyInfo.PropertyType == typeof(Int64)) {
        return propertyValue.GetInt64();
      } else if (prop.PropertyInfo.PropertyType == typeof(bool)) {
        return propertyValue.GetBoolean();
      } else if (prop.PropertyInfo.PropertyType == typeof(DateTime)) {
        return propertyValue.GetDateTime();
      } else if (prop.PropertyInfo.PropertyType == typeof(Int32)) {
        return propertyValue.GetInt32();
      } else {
        return null;
      }
    }
  }

  internal static class StringExtensions {
    public static string CapitalizeFirst(this string str) {
      return char.ToUpper(str[0]) + str.Substring(1);
    }
    public static string ToLowerFirst(this string str) {
      return char.ToLower(str[0]) + str.Substring(1);
    }
  }

}
