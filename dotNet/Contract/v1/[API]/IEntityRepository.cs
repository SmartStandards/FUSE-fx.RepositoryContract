
using System.Collections.Generic;
using System.Data.UDAS.v1.Models;

namespace System.Data.UDAS.v1 {
  public interface IEntityRepository<TEntity, TDbEntity, TSearchFilter> {
    TEntity AddOrUpdate(TEntity entity);
    void DeleteEntities(IList<object[]> keyValuesCollection);
    PaginatedResponse<TEntity> Search(ListSearchParams<TSearchFilter> searchParams);
    PaginatedResponse<TEntity> SearchByParentId(ListSearchParams<TSearchFilter> searchParams, long parentId);
  }
}
