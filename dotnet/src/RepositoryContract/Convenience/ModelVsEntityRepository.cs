using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Reflection;

namespace System.Data.Fuse.Convenience {
  public class ModelVsEntityRepositoryBase<TModel, TEntity>
    : IRepository<TModel>
    where TEntity : class
    where TModel : class {

    IRepository<TEntity> _Repository;
    private readonly Action<TModel, TEntity> _OnAfterModelToEntity;
    private readonly Action<TEntity, TModel> _OnAfterEntityToModel;

    private readonly Func<PropertyInfo, bool> _IsForeignKey;
    private readonly Func<PropertyInfo, bool> _IsNavigation;

    public ModelVsEntityRepositoryBase(
      IRepository<TEntity> repository,
      Action<TModel, TEntity> onAfterModelToEntity,
      Action<TEntity, TModel> onAfterEntityToModel,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      _Repository = repository;
      _OnAfterModelToEntity = onAfterModelToEntity;
      _OnAfterEntityToModel = onAfterEntityToModel;
      _IsForeignKey = isForeignKey;
      _IsNavigation = isNavigation;
    }

    public TModel AddOrUpdateEntity(TModel model) {
      return _Repository.AddOrUpdateEntity(
        model.ToEntity(this._IsForeignKey, this._IsNavigation, this._OnAfterModelToEntity)
      ).ToBusinessModel(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
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

    public IList<TModel> GetEntities(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
      return _Repository.GetEntities(
        filter, pagingParams, sortingParams
      ).ToBuseinssModels(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
    }

    public IList<TModel> GetEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
      return _Repository.GetEntities(
        dynamicLinqFilter, pagingParams, sortingParams
      ).ToBuseinssModels(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
    }

    public IList<EntityRefById> GetEntityRefs(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
      return _Repository.GetEntityRefs(
        filter, pagingParams, sortingParams
      );
    }

    public IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
      return _Repository.GetEntityRefs(
         dynamicLinqFilter, pagingParams, sortingParams
       );
    }
  }
}
