using System.Collections.Generic;

namespace System.Data.Fuse.WcfSupport {
  public class WcfRepositoryWrapper<TEntity, TKey> : IWcfRepository<TEntity, TKey>
     where TEntity : class {
    private readonly IRepository<TEntity, TKey> innerRepository;

    public WcfRepositoryWrapper(IRepository<TEntity, TKey> innerRepository) {
      this.innerRepository = innerRepository;
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      return innerRepository.AddOrUpdateEntity(entity);
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return innerRepository.AddOrUpdateEntityFields(fields);
    }

    public bool ContainsKey(TKey key) {
      return innerRepository.ContainsKey(key);
    }

    public int Count(ExpressionTree filter) {
      return innerRepository.Count(filter);
    }

    public int CountAll() {
      return innerRepository.CountAll();
    }

    public int CountBySearchExpression(string searchExpression) {
      return innerRepository.CountBySearchExpression(searchExpression);
    }

    public RepositoryCapabilities GetCapabilities() {
      return innerRepository.GetCapabilities();
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntities(filter, sortedBy, limit, skip);
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      return innerRepository.GetEntitiesByKey(keysToLoad);
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      return innerRepository.GetEntityFieldsByKey(keysToLoad, includedFieldNames);
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip);
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntityRefs(filter, sortedBy, limit, skip);
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return innerRepository.GetEntityRefsByKey(keysToLoad);
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return innerRepository.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public string GetOriginIdentity() {
      return innerRepository.GetOriginIdentity();
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return innerRepository.Massupdate(filter, fields);
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      return innerRepository.MassupdateByKeys(keysToUpdate, fields);
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return innerRepository.MassupdateBySearchExpression(searchExpression, fields);
    }

    public TKey TryAddEntity(TEntity entity) {
      return innerRepository.TryAddEntity(entity);
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      return innerRepository.TryDeleteEntities(keysToDelete);
    }

    public TEntity TryUpdateEntity(TEntity entity) {
      return innerRepository.TryUpdateEntity(entity);
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      return innerRepository.TryUpdateEntityFields(fields);
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      return innerRepository.TryUpdateKey(currentKey, newKey);
    }
  }
}
