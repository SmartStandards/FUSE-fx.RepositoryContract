//#if NETCOREAPP
//using Microsoft.EntityFrameworkCore;
//#else
//using System.Data.Entity;
//#endif
//using System.Collections;
//using System.Collections.Generic;
//using System.Data.Fuse.Logic;

//namespace System.Data.Fuse {

//  public abstract class EfRepositoryBase {
//    protected readonly DbContext context;

//    public EfRepositoryBase(DbContext dbContext) {
//      this.context = dbContext;
//    }

//    public abstract IList GetEntities1(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
//    public abstract IList GetEntities1(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

//    public abstract IList<Dictionary<string, object>> GetBusinessModels1(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
//    public abstract IList<Dictionary<string, object>> GetBusinessModels1(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

//    public abstract IList<EntityRef> GetEntityRefs1(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
//    public abstract IList<EntityRef> GetEntityRefs1(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

//    public abstract int GetCount1(LogicalExpression filter);

//    public abstract void DeleteEntities1(object[][] entityIdsToDelete);
//    public abstract object AddOrUpdate1(Dictionary<string, object> entity);
  
//  }
  
//}
