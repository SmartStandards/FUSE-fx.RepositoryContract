using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public interface IRepository<TDbEntity, TBusinessModel> where TDbEntity : class {

    IList<TDbEntity> GetDbEntities(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<TDbEntity> GetDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<TBusinessModel> GetBusinessModels(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<TBusinessModel> GetBusinessModels(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<EntityRefById> GetEntityRefs(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    TDbEntity AddOrUpdateEntity(TBusinessModel businessModel);

    void DeleteEntities(object[][] entityIdsToDelete);

  }

}
