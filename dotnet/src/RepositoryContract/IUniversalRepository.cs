using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public interface IUniversalRepository {

    IList GetEntities(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList GetEntities(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);
      
    IList<EntityRef> GetEntityRefs(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRef> GetEntityRefs(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    int GetCount(string entityName, LogicalExpression filter);
    int GetCount(string entityName, string dynamicLinqFilter);

    object AddOrUpdateEntity(string entityName, Dictionary<string, object> entity);
    void DeleteEntities(string entityName, object[][] entityIdsToDelete);

  }

}
