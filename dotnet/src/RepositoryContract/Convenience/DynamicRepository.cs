using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Data.Fuse.Convenience;

namespace System.Data.Fuse {

  public abstract class DynamicRepository : IDynamicRepository {

    private Dictionary<string, DynamicRepositoryFacade> _InnerRepos = new Dictionary<string, DynamicRepositoryFacade>();

    protected DynamicRepositoryFacade GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    //protected readonly DbContext _DbContext;
    //protected readonly Assembly _Assembly;

    //public EfRepository(DbContext dbContext, Assembly assembly) {
    //  this._DbContext = dbContext;
    //  this._Assembly = assembly;
    //}

    public object AddOrUpdateEntity(string entityName, Dictionary<string, object> entity) {
      return GetInnerRepo(entityName).AddOrUpdateEntity(entity);
    }

    public void DeleteEntities(string entityName, object[][] entityIdsToDelete) {
      GetInnerRepo(entityName).DeleteEntities(entityIdsToDelete);
    }
  
    public Collections.IList GetEntities(
      string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntities(filter, pagingParams, sortingParams);
    }

    public Collections.IList GetEntities(
      string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntities(dynamicLinqFilter, pagingParams, sortingParams);
    }

    public IList<EntityRefById> GetEntityRefs(
      string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return GetInnerRepo(entityName).GetEntityRefs(filter, pagingParams, sortingParams);
    }

    public IList<EntityRefById> GetEntityRefs(
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

    protected abstract DynamicRepositoryFacade CreateInnerRepo(string entityName);
//      {
//      Type[] allTypes = _Assembly.GetTypes();
//      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
//      if (entityType == null) { return null; }
//#if NETCOREAPP
//      Type repoType = typeof(EfRepository1<>);
//#else
//      Type repoType = typeof(EfRepository1_Alt<>);
//#endif
//      repoType = repoType.MakeGenericType(entityType);
//      return (EfRepositoryBase)Activator.CreateInstance(repoType, _DbContext);
//    }

  }

}
