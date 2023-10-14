﻿using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public interface IRepository<TDbEntity> where TDbEntity : class {

    IList<TDbEntity> GetDbEntities(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<TDbEntity> GetDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<Dictionary<string, object>> GetBusinessModels(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<Dictionary<string, object>> GetBusinessModels(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<EntityRefById> GetEntityRefs(LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    TDbEntity AddOrUpdateEntity(Dictionary<string, object> businessModel);

    void DeleteEntities(object[][] entityIdsToDelete);

  }

}
