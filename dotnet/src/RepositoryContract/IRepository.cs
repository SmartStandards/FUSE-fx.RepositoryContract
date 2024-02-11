using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public interface IRepository<TEntity> where TEntity : class {

    IList<TEntity> GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );
    IList<TEntity> GetEntities(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );
       
    IList<EntityRef> GetEntityRefs(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );
    IList<EntityRef> GetEntityRefs(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );

    int GetCount(LogicalExpression filter);
    int GetCount(string dynamicLinqFilter);

    TEntity AddOrUpdateEntity(TEntity entity);

    void DeleteEntities(object[][] entityIdsToDelete);

  }

}
