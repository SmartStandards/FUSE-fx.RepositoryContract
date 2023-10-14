using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public interface IRepository {

    IList GetDbEntities(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList GetDbEntities(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<Dictionary<string, object>> GetBusinessModels(
      string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );
    IList<Dictionary<string, object>> GetBusinessModels(
      string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );

    IList<EntityRef> GetEntityRefs(string entityName, LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRef> GetEntityRefs(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    int GetCount(string entityName, LogicalExpression filter);

    object AddOrUpdateEntity(string entityName, Dictionary<string, object> businessModel);
    void DeleteEntities(string entityName, object[][] entityIdsToDelete);

  }

}
