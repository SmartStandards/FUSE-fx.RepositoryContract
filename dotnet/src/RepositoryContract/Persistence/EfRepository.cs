//#if NETCOREAPP
//using Microsoft.EntityFrameworkCore;
//#else
//using System.Data.Entity;
//#endif
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using System.Linq;
//using System.Data.Fuse.Logic;

//namespace System.Data.Fuse {

//  public class EfRepository : IRepository {

//    private Dictionary<string, EfRepositoryBase> _InnerRepos = new Dictionary<string, EfRepositoryBase>();

//    protected EfRepositoryBase GetInnerRepo(string entityName) {
//      if (!_InnerRepos.ContainsKey(entityName)) {
//        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
//      }
//      return _InnerRepos[entityName];
//    }

//    protected readonly DbContext _DbContext;
//    protected readonly Assembly _Assembly;

//    public EfRepository(DbContext dbContext, Assembly assembly) {
//      this._DbContext = dbContext;
//      this._Assembly = assembly;
//    }

//    public object AddOrUpdateEntity(string entityName, Dictionary<string, object> entity) {
//      return GetInnerRepo(entityName).AddOrUpdate1(entity);
//    }

//    public void DeleteEntities(string entityName, object[][] entityIdsToDelete) {
//      GetInnerRepo(entityName).DeleteEntities1(entityIdsToDelete);
//    }

//    public IList<Dictionary<string, object>> GetBusinessModels(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetBusinessModels1(filter, pagingParams, sortingParams);
//    }

//    public IList<Dictionary<string, object>> GetBusinessModels(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetBusinessModels1(dynamicLinqFilter, pagingParams, sortingParams);
//    }

//    public Collections.IList GetDbEntities(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetEntities1(filter, pagingParams, sortingParams);
//    }

//    public Collections.IList GetDbEntities(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetEntities1(dynamicLinqFilter, pagingParams, sortingParams);
//    }

//    public IList<EntityRef> GetEntityRefs(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetEntityRefs1(filter, pagingParams, sortingParams);
//    }

//    public IList<EntityRef> GetEntityRefs(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams) {
//      return GetInnerRepo(entityName).GetEntityRefs1(dynamicLinqFilter, pagingParams, sortingParams);
//    }

//    public int GetCount(string entityName, LogicalExpression filter) {
//      return GetInnerRepo(entityName).GetCount1(filter);
//    }

//    private EfRepositoryBase CreateInnerRepo(string entityName) {
//      Type[] allTypes = _Assembly.GetTypes();
//      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
//      if (entityType == null) { return null; }
//#if NETCOREAPP
//      Type repoType = typeof(DbContextBasedEfRepository<>);
//#else
//      Type repoType = typeof(EntitySchemaBasedEfRepository<>);
//#endif
//      repoType = repoType.MakeGenericType(entityType);
//      return (EfRepositoryBase)Activator.CreateInstance(repoType, _DbContext);
//    }

//  }

//}
