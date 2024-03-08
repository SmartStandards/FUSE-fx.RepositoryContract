
//#if NETCOREAPP
//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.EntityFrameworkCore;
//#else
//using System.Data.Entity;
//#endif
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Linq;
//using System.Reflection;
//#if NETCOREAPP
//using System.Text.Json;
//#endif
//using System.Data.Fuse.Logic;
//using System.Collections;
//using System.Linq.Dynamic.Core;
//using System.Text;
//using System.Data.ModelDescription;
//using System.Data.Fuse.Persistence;

//namespace System.Data.Fuse {

//  public class EfRepository1_Alt<TEntity> : EfRepositoryBase, IRepository<TEntity>, ILinqRepository<TEntity> where TEntity : class, new() {
//    private readonly SchemaRoot _SchemaRoot;

//    public EfRepository1_Alt(DbContext context, SchemaRoot schemaRoot) : base(context) {
//      this._SchemaRoot = schemaRoot;
//    }

//    #region Forward Base Functions

//    public override object AddOrUpdateBase(Dictionary<string, object> entity) {
//      return AddOrUpdateEntity(entity);
//    }

//    public override void DeleteEntitiesBase(object[][] entityIdsToDelete) {
//      DeleteEntities(entityIdsToDelete);
//    }

//    public override IList GetEntitiesBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(filter.CompileToDynamicLinq(), pagingParams, sortingParams).ToList();
//    }

//    public override IList GetEntitiesBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList();
//    }

//    public override IList<Dictionary<string, object>> GetBusinessModelsBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IList entities = GetEntitiesBase(filter, pagingParams, sortingParams);
//      return ConvertToDtos(entities);
//    }

//    public override IList<Dictionary<string, object>> GetBusinessModelsBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IList entities = QueryDbEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList();
//      return ConvertToDtos(entities);
//    }

//    public override IList<EntityRef> GetEntityRefsBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IList entities = GetEntitiesBase(filter, pagingParams, sortingParams);
//      return ConvertToEntityRefs(entities);
//    }

//    public override IList<EntityRef> GetEntityRefsBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IList entities = GetEntitiesBase(dynamicLinqFilter, pagingParams, sortingParams);
//      return ConvertToEntityRefs(entities);
//    }

//    public override int GetCountBase(LogicalExpression filter) {
//      return QueryDbEntities(filter.CompileToDynamicLinq(), null, null).Count();
//    }

//    #endregion

//    #region IRepository1

//    public TEntity AddOrUpdateEntity(Dictionary<string, object> entity) {
//      //IEntityType efEntityType = context.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == typeof(TEntity).FullName);
//      //IKey primaryKey = efEntityType.GetKeys().First((k) => k.IsPrimaryKey());
//      //List<object> keyValues = new List<object>();
//      //foreach (IProperty keyProp in primaryKey.Properties) {
//      //  object keyPropValue = entity[keyProp.Name.ToLowerFirst()];
//      //  if (typeof(JsonElement).IsAssignableFrom(keyPropValue?.GetType())) {
//      //    JsonElement keyPropValueJson = (JsonElement)keyPropValue;
//      //    keyPropValue = GetValue(keyProp, keyPropValueJson);
//      //  }
//      //  keyValues.Add(keyPropValue);
//      //}

//      //TEntity existingEntity = context.Set<TEntity>().Find(keyValues.ToArray());

//      //if (existingEntity == null) {
//      //  return Add(entity);
//      //} else {
//      //  Utils.CopyProperties(entity, existingEntity);
//      //  context.SaveChanges();
//      //  return existingEntity;
//      //}
//      return null;
//    }

//    public void DeleteEntities(object[][] entityIdsToDelete) {
//      //foreach (object[] entityIdToDelete in entityIdsToDelete) {
//      //  if (entityIdsToDelete.Length == 0) {
//      //    continue;
//      //  }
//      //  object[] keysetToDelete = new object[entityIdsToDelete.Length];
//      //  IKey keyInfo = context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey();
//      //  if (keyInfo.Properties.Count() != keysetToDelete.Count()) { continue; }
//      //  int j = 0;
//      //  foreach (IProperty keyPropery in keyInfo.Properties) {
//      //    object keyPropValue = entityIdToDelete[j];
//      //    if (keyPropValue != null && typeof(JsonElement).IsAssignableFrom(keyPropValue.GetType())) {
//      //      JsonElement keyPropValueJson = (JsonElement)keyPropValue;
//      //      keyPropValue = GetValue(keyPropery, keyPropValueJson);
//      //    }
//      //    keysetToDelete[j] = keyPropValue;
//      //  }
//      //  TEntity entityToDelete = context.Set<TEntity>().Find(keysetToDelete);
//      //  if (entityToDelete == null) {
//      //    continue;
//      //  }
//      //  context.Set<TEntity>().Remove(entityToDelete);
//      //}
//      //context.SaveChanges();
//    }



//    public IList<EntityRefById> GetEntityRefs(PagingParams pagingParams, SortingField[] sortingParams) {
//      throw new NotImplementedException();
//    }

//    public IList<TEntity> GetDbEntities(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(filter.CompileToDynamicLinq(), pagingParams, sortingParams).ToList();
//    }

//    public IList<TEntity> GetDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList();
//    }

//    public IList<Dictionary<string, object>> GetBusinessModels(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return ConvertToDtos(GetDbEntities(filter, pagingParams, sortingParams).ToList());
//    }

//    public IList<Dictionary<string, object>> GetBusinessModels(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return ConvertToDtos(GetDbEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList());
//    }

//    public IList<EntityRefById> GetEntityRefs(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      throw new NotImplementedException();
//    }

//    public IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      throw new NotImplementedException();
//    }


//    #endregion

//    #region ILinqRepository

//    public IQueryable<TEntity> QueryDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IQueryable<TEntity> result;
//      if (string.IsNullOrEmpty(dynamicLinqFilter)) {
//        result = context.Set<TEntity>();
//      } else {
//        result = context.Set<TEntity>().Where(dynamicLinqFilter);
//      }

//      return ApplyPaging(ApplySorting(result, sortingParams), pagingParams);
//    }

//    public IQueryable<TEntity> QueryDbEntities(Expression<Func<TEntity, bool>> filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IQueryable<TEntity> result;
//      if (filter is null) {
//        result = context.Set<TEntity>();
//      } else {
//        result = context.Set<TEntity>().Where(filter);
//      }

//      return ApplyPaging(ApplySorting(result, sortingParams), pagingParams);
//    }

//    #endregion

//    #region Helper Functions

//    private IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> result, PagingParams pagingParams) {
//      if (pagingParams == null || pagingParams.PageSize == 0) {
//        return result;
//      }
//      int skip = pagingParams.PageSize * (pagingParams.PageNumber - 1);
//      return result.Skip(skip).Take(pagingParams.PageSize);
//    }

//    private IQueryable<TEntity> ApplySorting(IQueryable<TEntity> result, SortingField[] sortingParams) {
//      if (sortingParams == null || sortingParams.Count() == 0) {
//        return result;
//      }
//      StringBuilder sorting = new StringBuilder();
//      foreach (SortingField sortingField in sortingParams) {
//        if (sortingField.Descending) {
//          sorting.Append(sortingField.FieldName + " descending,");
//        } else {
//          sorting.Append(sortingField.FieldName + ",");
//        }
//      }
//      sorting.Length -= 1;
//      result = result.OrderBy(sorting.ToString());
//      return result;
//    }

//    private TEntity Add(Dictionary<string, object> entity) {
//      TEntity newEntity = new TEntity();
//      Utils.CopyProperties(entity, newEntity);
//      context.Set<TEntity>().Add(newEntity);
//      context.SaveChanges();
//      return newEntity;
//    }

//    private IList<Dictionary<string, object>> ConvertToDtos(IList entities) {
//      //Type entityType = typeof(TEntity);
//      //IEntityType efEntityType = context.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == entityType.FullName);

//      IList<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

//      //foreach (object entity in entities) {
//      //  Dictionary<string, object> dto = new Dictionary<string, object>();
//      //  foreach (IProperty scalarProperty in efEntityType.GetProperties()) {
//      //    if (scalarProperty.IsForeignKey()) { continue; }
//      //    dto.Add(scalarProperty.Name.ToLowerFirst(), entityType.GetProperty(scalarProperty.Name).GetValue(entity));
//      //  }
//      //  foreach (INavigation navProp in efEntityType.GetNavigations()) {
//      //    PropertyInfo navPropTarget = entityType.GetProperty(navProp.Name);
//      //    if (navPropTarget == null) { continue; }
//      //    object navPropValue = navPropTarget.GetValue(entity);
//      //    if (navPropValue == null) {
//      //      dto.Add(navProp.Name.ToLowerFirst(), null);
//      //      continue;
//      //    }
//      //    PropertyInfo navIdProp = navPropValue.GetType().GetProperty("Id");
//      //    if (navIdProp == null) { continue; }
//      //    object navId = navIdProp.GetValue(navPropValue);
//      //    EntityRefById entityRef = new EntityRefById();
//      //    entityRef.Id = navId.ToString();
//      //    entityRef.Label = navPropValue.ToString();
//      //    dto.Add(navProp.Name.ToLowerFirst(), entityRef);
//      //  }
//      //  result.Add(dto);
//      //}
//      return result;
//    }

//    private IList<EntityRef> ConvertToEntityRefs(IList entities) {
//      //Type entityType = typeof(TEntity);
//      //IEntityType efEntityType = context.Model.GetEntityTypes().FirstOrDefault((et) => et.Name == entityType.FullName);

//      IList<EntityRef> result = new List<EntityRef>();

//      //IKey primaryKey = efEntityType.FindPrimaryKey();
//      //if (primaryKey == null) {
//      //  return result;
//      //}
//      //IReadOnlyList<IProperty> primaryKeyProperties = primaryKey.Properties;
//      //foreach (object entity in entities) {
//      //  List<object> primaryKeyValues = new List<object>();
//      //  foreach (IProperty primaryKeyProperty in primaryKeyProperties) {
//      //    object primaryKeyValue = primaryKeyProperty.PropertyInfo.GetValue(entity, null);
//      //    primaryKeyValues.Add(primaryKeyValue);
//      //  }
//      //  EntityRef entityRef = new EntityRef() {
//      //    KeyValues = primaryKeyValues.ToArray(),
//      //    Label = entity.ToString()
//      //  };
//      //  result.Add(entityRef);
//      //}
//      return result;
//    }

//    //private object GetValue(IProperty prop, JsonElement propertyValue) {
//    //  if (prop.PropertyInfo.PropertyType == typeof(string)) {
//    //    return propertyValue.GetString();
//    //  } else if (prop.PropertyInfo.PropertyType == typeof(Int64)) {
//    //    return propertyValue.GetInt64();
//    //  } else if (prop.PropertyInfo.PropertyType == typeof(bool)) {
//    //    return propertyValue.GetBoolean();
//    //  } else if (prop.PropertyInfo.PropertyType == typeof(DateTime)) {
//    //    return propertyValue.GetDateTime();
//    //  } else if (prop.PropertyInfo.PropertyType == typeof(Int32)) {
//    //    return propertyValue.GetInt32();
//    //  } else {
//    //    return null;
//    //  }
//    //}

//    #endregion

//  }

//}

