using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Linq;

namespace System.Data.Fuse {
 
  //AI
  public abstract class UniversalRepository : IUniversalRepository {
    private Dictionary<string, UniversalRepositoryFacade> _InnerRepos = new Dictionary<string, UniversalRepositoryFacade>();

    protected UniversalRepositoryFacade GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    protected abstract UniversalRepositoryFacade CreateInnerRepo(string entityName);

    public string GetOriginIdentity() {
      return string.Empty;
    }

    public RepositoryCapabilities GetCapabilities() {
      return new RepositoryCapabilities();
    }

    public string[] GetEntityNames() {
      return _InnerRepos.Keys.ToArray();
    }

    public EntityRef[] GetEntityRefs(string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntityRefs(filter, sortedBy, limit, skip);
    }

    public EntityRef[] GetEntityRefsBySearchExpression(string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public EntityRef[] GetEntityRefsByKey(string entityName, object[] keysToLoad) {
      return GetInnerRepo(entityName).GetEntityRefsByKey(keysToLoad);
    }

    public object[] GetEntities(string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntities(filter, sortedBy, limit, skip);
    }

    public object[] GetEntitiesBySearchExpression(string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip);
    }

    public object[] GetEntitiesByKey(string entityName, object[] keysToLoad) {
      return GetInnerRepo(entityName).GetEntitiesByKey(keysToLoad);
    }

    public Dictionary<string, object>[] GetEntityFields(string entityName, ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string entityName, string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return GetInnerRepo(entityName).GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip);
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(string entityName, object[] keysToLoad, string[] includedFieldNames) {
      return GetInnerRepo(entityName).GetEntityFieldsByKey(keysToLoad, includedFieldNames);
    }

    public int CountAll(string entityName) {
      return GetInnerRepo(entityName).CountAll();
    }

    public int Count(string entityName, ExpressionTree filter) {
      return GetInnerRepo(entityName).Count(filter);
    }

    public int CountBySearchExpression(string entityName, string searchExpression) {
      return GetInnerRepo(entityName).CountBySearchExpression(searchExpression);
    }

    public bool ContainsKey(string entityName, object key) {
      return GetInnerRepo(entityName).ContainsKey(key);
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
      return GetInnerRepo(entityName).AddOrUpdateEntityFields(fields);
    }

    public object AddOrUpdateEntity(string entityName, object entity) {
      return GetInnerRepo(entityName).AddOrUpdateEntity(entity);
    }

    public Dictionary<string, object> TryUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
      return GetInnerRepo(entityName).TryUpdateEntityFields(fields);
    }

    public object TryUpdateEntity(string entityName, object entity) {
      return GetInnerRepo(entityName).TryUpdateEntity(entity);
    }

    public object TryAddEntity(string entityName, object entity) {
      return GetInnerRepo(entityName).TryAddEntity(entity);
    }

    public object[] MassupdateByKeys(string entityName, object[] keysToUpdate, Dictionary<string, object> fields) {
      return GetInnerRepo(entityName).MassupdateByKeys(keysToUpdate, fields);
    }

    public object[] Massupdate(string entityName, ExpressionTree filter, Dictionary<string, object> fields) {
      return GetInnerRepo(entityName).Massupdate(filter, fields);
    }

    public object[] MassupdateBySearchExpression(string entityName, string searchExpression, Dictionary<string, object> fields) {
      return GetInnerRepo(entityName).MassupdateBySearchExpression(searchExpression, fields);
    }

    public object[] TryDeleteEntities(string entityName, object[] keysToDelete) {
      return GetInnerRepo(entityName).TryDeleteEntities(keysToDelete);
    }

    public bool TryUpdateKey(string entityName, object currentKey, object newKey) {
      return GetInnerRepo(entityName).TryUpdateKey(currentKey, newKey);
    }
  }

}
