using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text.Json;

namespace System.Data.Fuse {

  public interface IRepository<TDbEntity> where TDbEntity : class {

    IList<TDbEntity> GetDbEntities(SimpleExpressionTree filter);
    IList<TDbEntity> GetDbEntities(string dynamicLinqFilter);

    IList<Dictionary<string, object>> GetBusinessModels(SimpleExpressionTree filter);
    IList<Dictionary<string, object>> GetBusinessModels(string dynamicLinqFilter);

    IList<EntityRefById> GetEntityRefs(SimpleExpressionTree filter);
    IList<EntityRefById> GetEntityRefs(string dynamicLinqFilter);

    TDbEntity AddOrUpdateEntity(Dictionary<string, JsonElement> businessModel);

    void DeleteEntities(JsonElement[][] entityIdsToDelete);

  }

}
