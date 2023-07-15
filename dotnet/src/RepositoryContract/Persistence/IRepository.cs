using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text.Json;

namespace System.Data.Fuse {

  public interface IRepository {

    IList GetDbEntities(string entityName, SimpleExpressionTree filter);
    IList GetDbEntities(string entityName, string dynamicLinqFilter);

    IList<Dictionary<string, object>> GetBusinessModels(string entityName, SimpleExpressionTree filter);
    IList<Dictionary<string, object>> GetBusinessModels(string entityName, string dynamicLinqFilter);

    IList<EntityRefById> GetEntityRefs(string entityName);

    object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> businessModel);
    void DeleteEntities(string entityName, JsonElement[][] entityIdsToDelete);

  }

}
