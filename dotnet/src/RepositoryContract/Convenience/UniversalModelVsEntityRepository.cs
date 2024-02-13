using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Reflection;

namespace System.Data.Fuse.Convenience {
  public class UniversalModelVsEntityRepository<TEntity>
    : IRepository<Dictionary<string, object>>
    where TEntity : class
    {

    IRepository<TEntity> _Repository;

    private readonly Func<PropertyInfo, bool> _IsForeignKey;
    private readonly Func<PropertyInfo, bool> _IsNavigation;

    public UniversalModelVsEntityRepository(
      IRepository<TEntity> repository,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      _Repository = repository;
      _IsForeignKey = isForeignKey;
      _IsNavigation = isNavigation;
    }

    public Dictionary<string, object> AddOrUpdateEntity(Dictionary<string, object> model) {
      return _Repository.AddOrUpdateEntity(
        model.ConvertToEntityDynamic<TEntity>(this._IsForeignKey, this._IsNavigation)
      ).ConvertToBusinessModelDynamic(this._IsForeignKey, this._IsNavigation);
    }

    public void DeleteEntities(object[][] entityIdsToDelete) {
      _Repository.DeleteEntities(entityIdsToDelete);
    }

    public int GetCount(LogicalExpression filter) {
      return _Repository.GetCount(filter);
    }

    public int GetCount(string dynamicLinqFilter) {
      return _Repository.GetCount(dynamicLinqFilter);
    }

    public IList<Dictionary<string, object>> GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _Repository.GetEntities(
        filter, pagingParams, sortingParams
      ).ToBusinessModelsDynamic(this._IsForeignKey, this._IsNavigation);
    }

    public IList<Dictionary<string, object>> GetEntities(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _Repository.GetEntities(
        dynamicLinqFilter, pagingParams, sortingParams
      ).ToBusinessModelsDynamic(this._IsForeignKey, this._IsNavigation);
    }

    public IList<EntityRef> GetEntityRefs(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _Repository.GetEntityRefs(
        filter, pagingParams, sortingParams
      );
    }

    public IList<EntityRef> GetEntityRefs(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _Repository.GetEntityRefs(
         dynamicLinqFilter, pagingParams, sortingParams
       );
    }
  }
}
