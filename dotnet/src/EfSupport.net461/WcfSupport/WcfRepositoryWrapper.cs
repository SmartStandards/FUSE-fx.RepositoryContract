using System.Collections.Generic;

namespace System.Data.Fuse.WcfSupport {
  public class WcfRepositoryWrapper<TEntity, TKey> : IRepository<TEntity, TKey>
     where TEntity : class {
    private readonly IWcfRepository<TEntity, TKey> wcfRepository;

    public WcfRepositoryWrapper(IWcfRepository<TEntity, TKey> wcfRepository) {
      this.wcfRepository = wcfRepository;
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      return wcfRepository.AddOrUpdateEntity(entity);
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return wcfRepository.AddOrUpdateEntityFields(fields);
    }

    public bool ContainsKey(TKey key) {
      return wcfRepository.ContainsKey(key);
    }

    public int Count(ExpressionTree filter) {
      return wcfRepository.Count(filter);
    }

    public int CountAll() {
      return wcfRepository.CountAll();
    }

    public int CountBySearchExpression(string searchExpression) {
      return wcfRepository.CountBySearchExpression(searchExpression);
    }

    public RepositoryCapabilities GetCapabilities() {
      return wcfRepository.GetCapabilities();
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntities(filter, sortedBy, limit, skip);
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      return wcfRepository.GetEntitiesByKey(keysToLoad);
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      return wcfRepository.GetEntityFieldsByKey(keysToLoad, includedFieldNames);
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip);
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntityRefs(filter, sortedBy, limit, skip);
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return wcfRepository.GetEntityRefsByKey(keysToLoad);
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return wcfRepository.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public string GetOriginIdentity() {
      return wcfRepository.GetOriginIdentity();
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return wcfRepository.Massupdate(filter, fields);
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      return wcfRepository.MassupdateByKeys(keysToUpdate, fields);
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return wcfRepository.MassupdateBySearchExpression(searchExpression, fields);
    }

    public TKey TryAddEntity(TEntity entity) {
      return wcfRepository.TryAddEntity(entity);
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      return wcfRepository.TryDeleteEntities(keysToDelete);
    }

    public TEntity TryUpdateEntity(TEntity entity) {
      return wcfRepository.TryUpdateEntity(entity);
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      return wcfRepository.TryUpdateEntityFields(fields);
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      return wcfRepository.TryUpdateKey(currentKey, newKey);
    }
  }
}
