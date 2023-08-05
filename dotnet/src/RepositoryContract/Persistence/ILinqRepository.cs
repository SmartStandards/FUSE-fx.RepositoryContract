using System.Linq.Expressions;
using System.Linq;

namespace System.Data.Fuse {

  public interface ILinqRepository<TDbEntity> {
    IQueryable<TDbEntity> QueryDbEntities(Expression<Func<TDbEntity, bool>> filter, PagingParams pagingParams,SortingField[] sortingParams);

    IQueryable<TDbEntity> QueryDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);
  }

}