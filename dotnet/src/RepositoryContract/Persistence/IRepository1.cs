using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text.Json;

namespace System.Data.Fuse {

  public interface IRepository<TDbEntity> where TDbEntity : class {

    IList<TDbEntity> GetDbEntities(SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<TDbEntity> GetDbEntities(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<Dictionary<string, object>> GetBusinessModels(SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<Dictionary<string, object>> GetBusinessModels(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    IList<EntityRefById> GetEntityRefs(SimpleExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams);
    IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams);

    TDbEntity AddOrUpdateEntity(Dictionary<string, JsonElement> businessModel);

    void DeleteEntities(JsonElement[][] entityIdsToDelete);

  }

}
