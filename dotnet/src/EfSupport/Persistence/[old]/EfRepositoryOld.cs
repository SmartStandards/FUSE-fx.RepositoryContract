
//#if NETCOREAPP
//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;
//#else
//using System.Data.Entity;
//#endif

//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Linq;
//using System.Reflection;
//using System.Data.Fuse.Logic;
//using System.Linq.Dynamic.Core;
//using System.Text;
//using System.Data.Fuse.Convenience;
//using System.Data.ModelDescription;
//using System.Data.ModelDescription.Convenience;

//#if NETCOREAPP

//namespace System.Data.Fuse.Ef {

//  public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>, ILinqRepository<TEntity>
//    where TEntity : class, new() {

//    private DbContext _DContext;
//    private static SchemaRoot _SchemaRoot;

//    public EfRepository(DbContext context) {
//      _DContext = context;
//      _SchemaRoot = System.Data.Fuse.ModelReader.GetSchema(typeof(TEntity).Assembly, typeof(TEntity).Namespace);
//    }

//    private static List<List<PropertyInfo>> _UniqueKeySets;
//    private static List<PropertyInfo> _PrimaryKeySet;

//    protected List<List<PropertyInfo>> Keysets {
//      get {
//        if (_UniqueKeySets == null) {
//          _UniqueKeySets = InitUniqueKeySets();
//        }
//        return _UniqueKeySets;
//      }
//    }

//    protected List<PropertyInfo> PrimaryKeySet {
//      get {
//        if (_PrimaryKeySet == null) {
//          _PrimaryKeySet = InitPrimaeyKeySet();
//        }
//        return _PrimaryKeySet;
//      }
//    }

//    protected List<List<PropertyInfo>> InitUniqueKeySets() {
//      return _SchemaRoot.GetUniqueKeysetsProperties(typeof(TEntity));
//    }

//    protected List<PropertyInfo> InitPrimaeyKeySet() {
//      return _SchemaRoot.GetPrimaryKeyProperties(typeof(TEntity)).ToList();
//    }

//    //#if NETCOREAPP
//    //    protected virtual List<List<PropertyInfo>> GetKeySets() {

//    //      if (_KeySets != null) return _KeySets;

//    //      List<List<PropertyInfo>> result = new List<List<PropertyInfo>>();
//    //      List<PropertyInfo> primaryKeySet = new List<PropertyInfo>();

//    //      IEntityType efEntityType = this._DContext.Model.GetEntityTypes().FirstOrDefault(
//    //        (et) => et.Name == typeof(TEntity).FullName
//    //      );
//    //      IKey primaryKey = efEntityType.GetKeys().First((k) => k.IsPrimaryKey());
//    //      primaryKeySet = primaryKey.Properties.Select(p => p.PropertyInfo).ToList();
//    //      result.Add(primaryKeySet);
//    //      _KeySets = result;
//    //      return result;
//    //    }
//    //#else
//    //    protected virtual List<List<PropertyInfo>> GetKeySets() {

//    //      if (_KeySets != null) return _KeySets;

//    //      List<List<PropertyInfo>> result = new List<List<PropertyInfo>>();
//    //      List<PropertyInfo> idSet = new List<PropertyInfo>();
//    //      PropertyInfo idProp = typeof(TEntity).GetProperty("Id");
//    //      if (idProp != null) {
//    //        idSet.Add(idProp);
//    //      }
//    //      result.Add(idSet);
//    //      _KeySets = result;
//    //      return result;
//    //    }
//    //#endif


//    #region IRepository

//    private object[] GetValues(List<PropertyInfo> properties, TEntity entity) {
//      List<object> values = new List<object>();
//      foreach (PropertyInfo propertyInfo in properties) {
//        values.Add(propertyInfo.GetValue(entity, null));
//      }
//      return values.ToArray();
//    }

//    public TEntity AddOrUpdateEntity(TEntity entity) {
//      //IEntityType efEntityType = this._DContext.Model.GetEntityTypes().FirstOrDefault(
//      //  (et) => et.Name == typeof(TEntity).FullName
//      //);

//      object[] keySetValues = GetValues(PrimaryKeySet, entity);
//      TEntity existingEntity = this._DContext.Set<TEntity>().Find(keySetValues.ToArray());

//      //foreach (IProperty keyProp in primaryKey.Properties) {
//      //  object keyPropValue = entity[keyProp.Name.ToLowerFirst()];
//      //  if (typeof(JsonElement).IsAssignableFrom(keyPropValue?.GetType())) {
//      //    JsonElement keyPropValueJson = (JsonElement)keyPropValue;
//      //    keyPropValue = GetValue(keyProp, keyPropValueJson);
//      //  }
//      //  keyValues.Add(keyPropValue);
//      //}

//      if (existingEntity == null) {
//        _DContext.Set<TEntity>().Add(entity);
//        _DContext.SaveChanges();
//        return entity;
//      } else {
//        CopyFields(entity, existingEntity);
//        _DContext.SaveChanges();
//        return existingEntity;
//      }
//    }

//    public void DeleteEntities(object[][] entityIdsToDelete) {
//      foreach (object[] entityIdToDelete in entityIdsToDelete) {
//        if (entityIdsToDelete.Length == 0) {
//          continue;
//        }
//        TEntity existingEntity = this._DContext.Set<TEntity>().Find(entityIdsToDelete);
//        if (existingEntity != null) {
//          _DContext.Set<TEntity>().Remove(existingEntity);
//        }
//      }
//      _DContext.SaveChanges();
//    }

//    public IList<TEntity> GetEntities(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(filter.CompileToDynamicLinq(), pagingParams, sortingParams).ToList();
//    }

//    public IList<TEntity> GetEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return QueryDbEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList();
//    }

//    public IList<EntityRefById> GetEntityRefs(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      throw new NotImplementedException();
//    }

//    public IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      throw new NotImplementedException();
//    }

//    public int GetCount(LogicalExpression filter) {
//      throw new NotImplementedException();
//    }

//    public int GetCount(string dynamicLinqFilter) {
//      throw new NotImplementedException();
//    }

//    #endregion

//    #region ILinqRepository

//    public IList<TEntity> GetEntities(
//      Expression<Func<TEntity, bool>> filter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      return this.QueryDbEntities(filter, pagingParams, sortingParams).ToList();
//    }

//    public IList<EntityRefById> GetEntityRefs(
//      Expression<Func<TEntity, bool>> filter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      throw new NotImplementedException();
//    }

//    public int GetCount(Expression<Func<TEntity, bool>> filter) {
//      return this.QueryDbEntities(filter, null, null).Count();
//    }

//    #endregion

//    #region IQueryRepository

//    public IQueryable<TEntity> QueryDbEntities(
//      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      IQueryable<TEntity> result;
//      if (string.IsNullOrEmpty(dynamicLinqFilter)) {
//        result = this._DContext.Set<TEntity>();
//      } else {
//        result = this._DContext.Set<TEntity>().Where(dynamicLinqFilter);
//      }

//      return ApplyPaging(ApplySorting(result, sortingParams), pagingParams);
//    }

//    public IQueryable<TEntity> QueryDbEntities(Expression<Func<TEntity, bool>> filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      IQueryable<TEntity> result;
//      if (filter is null) {
//        result = this._DContext.Set<TEntity>();
//      } else {
//        result = this._DContext.Set<TEntity>().Where(filter);
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

//    private void CopyFields(TEntity from, TEntity to) {
//      EntitySchema schema = _SchemaRoot.GetSchema(typeof(TEntity).Name);
//      foreach (PropertyInfo propertyInfo in typeof(TEntity).GetProperties()) {
//        if (!schema.Fields.Any((f) => f.Name == propertyInfo.Name)) continue;
//        propertyInfo.SetValue(to, propertyInfo.GetValue(from, null), null);
//      }
//    }

//    #endregion

//  }

//}

//#endif