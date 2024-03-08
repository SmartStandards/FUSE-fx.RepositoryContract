#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Collections;
using System.Collections.Generic;

namespace System.Data.Fuse.Persistence {

  public interface IEfRepositoryUntyped  {
    //protected readonly DbContext context;

    //public EfRepositoryBase(DbContext dbContext) {
    //  this.context = dbContext;
    //}

    //IList GetEntitiesBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    //IList GetEntitiesBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    //IList<Dictionary<string, object>> GetBusinessModelsBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    //IList<Dictionary<string, object>> GetBusinessModelsBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    //IList<EntityRef> GetEntityRefsBase(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    //IList<EntityRef> GetEntityRefsBase(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    //int GetCountBase(LogicalExpression filter);

    //void DeleteEntitiesBase(object[][] entityIdsToDelete);
    //object AddOrUpdateBase(Dictionary<string, object> entity);

  }
}
