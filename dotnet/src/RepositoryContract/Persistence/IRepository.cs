using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text.Json;

namespace System.Data.Fuse {

  public interface IRepository {

    IList GetDbEntities(string entityName, SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList GetDbEntities(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<Dictionary<string, object>> GetBusinessModels(
      string entityName, SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
    );
    IList<Dictionary<string, object>> GetBusinessModels(
      string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );

    IList<EntityRefById> GetEntityRefs(string entityName, SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRefById> GetEntityRefs(string entityName, string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    int GetCount(string entityName, SimpleExpressionTree filter);

    object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> businessModel);
    void DeleteEntities(string entityName, JsonElement[][] entityIdsToDelete);

  }

}
