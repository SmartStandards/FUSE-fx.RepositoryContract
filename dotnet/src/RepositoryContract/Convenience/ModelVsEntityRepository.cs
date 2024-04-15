using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public class ModelVsEntityParams<TModel, TEntity> {
    public readonly Action<TModel, TEntity> OnAfterModelToEntity = (m, e) => { };
    public readonly Action<TEntity, TModel> OnAfterEntityToModel = (e, m) => { };

    public readonly Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> HandlePropertyModelToEntity = (pi, m, e) => false;
    public readonly Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> HandlePropertyEntityToModel = (pi, e, m) => false;
  }

  //AI
  public class ModelVsEntityRepository<TModel, TEntity, TKey>
    : IRepository<TModel, TKey>
    where TEntity : class
    where TModel : class {

    IRepository<TEntity, TKey> _Repository;
    private readonly Action<TModel, TEntity> _OnAfterModelToEntity;
    private readonly Action<TEntity, TModel> _OnAfterEntityToModel;

    private readonly Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> _HandlePropertyModelToEntity;
    private readonly Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> _HandlePropertyEntityToModel;

    public ModelVsEntityRepository(
      IRepository<TEntity, TKey> repository,
      Action<TModel, TEntity> onAfterModelToEntity,
      Action<TEntity, TModel> onAfterEntityToModel,
      Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> handlePropertyModelToEntity,
      Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> handlePropertyEntityToModel
    ) {
      _Repository = repository;
      _OnAfterModelToEntity = onAfterModelToEntity;
      _OnAfterEntityToModel = onAfterEntityToModel;
      _HandlePropertyModelToEntity = handlePropertyModelToEntity;
      _HandlePropertyEntityToModel = handlePropertyEntityToModel;
    }

    public ModelVsEntityRepository(
      IRepository<TEntity, TKey> repository,
      ModelVsEntityParams<TModel, TEntity> modelVsEntityParams
    ) {
      _Repository = repository;
      _OnAfterModelToEntity = modelVsEntityParams.OnAfterModelToEntity;
      _OnAfterEntityToModel = modelVsEntityParams.OnAfterEntityToModel;
      _HandlePropertyModelToEntity = modelVsEntityParams.HandlePropertyModelToEntity;
      _HandlePropertyEntityToModel = modelVsEntityParams.HandlePropertyEntityToModel;
    }

    public TModel AddOrUpdateEntity(TModel entity) {
      return _Repository.AddOrUpdateEntity(
        entity.ToEntity(_HandlePropertyModelToEntity, _OnAfterModelToEntity)
      ).ToBusinessModel(_HandlePropertyEntityToModel, _OnAfterEntityToModel);
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return _Repository.AddOrUpdateEntityFields(fields);
    }

    public bool ContainsKey(TKey key) {
      return _Repository.ContainsKey(key);
    }

    public int Count(ExpressionTree filter) {
      return _Repository.Count(filter);
    }

    public int CountAll() {
      return _Repository.CountAll();
    }

    public int CountBySearchExpression(string searchExpression) {
      return _Repository.CountBySearchExpression(searchExpression);
    }

    public RepositoryCapabilities GetCapabilities() {
      return _Repository.GetCapabilities();
    }

    public TModel[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _Repository.GetEntities(filter, sortedBy, limit, skip)
        .Select((e) => e.ToBusinessModel(_HandlePropertyEntityToModel, _OnAfterEntityToModel))
        .ToArray();
    }

    public TModel[] GetEntitiesByKey(TKey[] keysToLoad) {
      return _Repository.GetEntitiesByKey(keysToLoad)
        .Select((e) => e.ToBusinessModel(_HandlePropertyEntityToModel, _OnAfterEntityToModel))
        .ToArray();
    }

    public TModel[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip)
          .Select((e) => e.ToBusinessModel(_HandlePropertyEntityToModel, _OnAfterEntityToModel))
          .ToArray()
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter,
      string[] includedFieldNames,
      string[] sortedBy,
      int limit = 100, int skip = 0
    ) {
      return _Repository.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(
      TKey[] keysToLoad, string[] includedFieldNames
    ) {
      return _Repository.GetEntityFieldsByKey(keysToLoad, includedFieldNames);
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip)
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _Repository.GetEntityRefs(filter, sortedBy, limit, skip);
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return _Repository.GetEntityRefsByKey(keysToLoad);
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip)
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    public string GetOriginIdentity() {
      return _Repository.GetOriginIdentity();
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return _Repository.Massupdate(filter, fields);
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      return _Repository.MassupdateByKeys(keysToUpdate, fields);
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return _Repository.MassupdateBySearchExpression(searchExpression, fields);
    }

    public TKey TryAddEntity(TModel entity) {
      return _Repository.TryAddEntity(entity.ToEntity<TModel, TEntity>(_HandlePropertyModelToEntity, _OnAfterModelToEntity));
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      return _Repository.TryDeleteEntities(keysToDelete);
    }

    public TModel TryUpdateEntity(TModel entity) {
      return _Repository.TryUpdateEntity(entity.ToEntity<TModel, TEntity>(_HandlePropertyModelToEntity, _OnAfterModelToEntity))
        .ToBusinessModel(_HandlePropertyEntityToModel, _OnAfterEntityToModel);
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      return _Repository.TryUpdateEntityFields(fields);
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      return _Repository.TryUpdateKey(currentKey, newKey);
    }
  }

}
