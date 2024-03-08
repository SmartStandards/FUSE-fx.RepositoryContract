using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public class ModelVsEntityRepositoryBase<TModel, TEntity, TKey> { 
    //: IRepository<TModel, TKey>
    //where TEntity : class
    //where TModel : class {

    //IRepository<TEntity, TKey> _Repository;
    private readonly Action<TModel, TEntity> _OnAfterModelToEntity;
    private readonly Action<TEntity, TModel> _OnAfterEntityToModel;

    private readonly Func<PropertyInfo, bool> _IsForeignKey;
    private readonly Func<PropertyInfo, bool> _IsNavigation;

    //public ModelVsEntityRepositoryBase(
    //  IRepository<TEntity, TKey> repository,
    //  Action<TModel, TEntity> onAfterModelToEntity,
    //  Action<TEntity, TModel> onAfterEntityToModel,
    //  Func<PropertyInfo, bool> isForeignKey,
    //  Func<PropertyInfo, bool> isNavigation
    //) {
    //  //_Repository = repository;
    //  _OnAfterModelToEntity = onAfterModelToEntity;
    //  _OnAfterEntityToModel = onAfterEntityToModel;
    //  _IsForeignKey = isForeignKey;
    //  _IsNavigation = isNavigation;
    //}

    //public TModel AddOrUpdateEntity(TModel model) {
    //  return _Repository.AddOrUpdateEntity(
    //    model.ToEntity(this._IsForeignKey, this._IsNavigation, this._OnAfterModelToEntity)
    //  ).ToBusinessModel(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
    //}

    //public void DeleteEntities(object[][] entityIdsToDelete) {
    //  _Repository.DeleteEntities(entityIdsToDelete);
    //}

    //public int GetCount(ExpressionTree filter) {
    //  return _Repository.GetCount(filter);
    //}

    //public int GetCount(string dynamicLinqFilter) {
    //  return _Repository.GetCount(dynamicLinqFilter);
    //}

    //public IList<TModel> GetEntities(
    //  ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _Repository.GetEntities(
    //    filter, pagingParams, sortingParams
    //  ).ToBuseinssModels(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
    //}

    //public IList<TModel> GetEntities(
    //  string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _Repository.GetEntities(
    //    dynamicLinqFilter, pagingParams, sortingParams
    //  ).ToBuseinssModels(this._IsForeignKey, this._IsNavigation, this._OnAfterEntityToModel);
    //}

    //public IList<EntityRef> GetEntityRefs(
    //  ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _Repository.GetEntityRefs(
    //    filter, pagingParams, sortingParams
    //  );
    //}

    //public IList<EntityRef> GetEntityRefs(
    //  string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _Repository.GetEntityRefs(
    //     dynamicLinqFilter, pagingParams, sortingParams
    //   );
    //}
    
  }

}
