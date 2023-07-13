using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Text.Json;

//TODO: rename namespace of the final versions to "System.Data.Fuse"
namespace System.Data.Fuse {

  public interface IGenericRepository {

    IList GetEntities(string entityName, SimpleExpressionTree filter);
    IList GetEntities(string entityName, string dynamicLinqFilter);

    IList<Dictionary<string, object>> GetDtos(string entityName, SimpleExpressionTree filter);
    IList<Dictionary<string, object>> GetDtos(string entityName, string dynamicLinqFilter);

    IList<EntityRefById> GetEntityRefs(string entityName);

    object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> entity);
    void DeleteEntities(string entityName, JsonElement[][] entityIdsToDelete);

  }

}
