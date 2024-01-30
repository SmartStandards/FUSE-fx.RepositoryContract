using System.Linq.Expressions;
using System.Collections.Generic;

namespace System.Data.Fuse {

  public interface ILinqRepository<TEntity> {
    IList<TEntity> GetEntities(Expression<Func<TEntity, bool>> filter, PagingParams pagingParams,SortingField[] sortingParams);
    IList<EntityRefById> GetEntityRefs(
      Expression<Func<TEntity, bool>> filter, PagingParams pagingParams, SortingField[] sortingParams
    );
    int GetCount(Expression<Func<TEntity, bool>> filter);
    TEntity AddOrUpdateEntity(TEntity entity);
    void DeleteEntities(object[][] entityIdsToDelete);
  }

}