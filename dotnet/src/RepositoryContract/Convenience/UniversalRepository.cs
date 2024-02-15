using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Data.Fuse.Convenience;

namespace System.Data.Fuse {

  public abstract class UniversalRepository : IUniversalRepository {

    private Dictionary<string, UniversalRepositoryFacade> _InnerRepos = new Dictionary<string, UniversalRepositoryFacade>();

    protected UniversalRepositoryFacade GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    public object AddOrUpdateEntity(string entityName, Dictionary<string, object> entity) {
      return GetInnerRepo(entityName).AddOrUpdateEntity(entity);
    }

    public void DeleteEntities(string entityName, object[][] entityIdsToDelete) {
      GetInnerRepo(entityName).DeleteEntities(entityIdsToDelete);
    }
  
    public IList<Dictionary<string, object>> GetEntities(
      string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntities(filter, pagingParams, sortingParams);
    }

    public IList<Dictionary<string, object>> GetEntities(
      string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntities(dynamicLinqFilter, pagingParams, sortingParams);
    }

    public IList<EntityRef> GetEntityRefs(
      string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntityRefs(filter, pagingParams, sortingParams);
    }

    public IList<EntityRef> GetEntityRefs(
      string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntityRefs(dynamicLinqFilter, pagingParams, sortingParams);
    }

    public int GetCount(string entityName, LogicalExpression filter) {
      return GetInnerRepo(entityName).GetCount(filter);
    }

    public int GetCount(string entityName, string dynamicLinqfilter) {
      return GetInnerRepo(entityName).GetCount(dynamicLinqfilter);
    }

    protected abstract UniversalRepositoryFacade CreateInnerRepo(string entityName);

  }

}
