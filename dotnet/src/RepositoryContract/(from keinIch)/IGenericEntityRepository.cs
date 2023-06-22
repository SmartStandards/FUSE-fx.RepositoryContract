using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

//TODO: rename namespace of the final versions to "System.Data.Fuse"
namespace System.Data.Fuse {

  public partial interface IGenericEntityRepository {

    IList GetEntities(string entityName);
    IList<Dictionary<string, object>> GetDtos(string entityName);

    IList<EntityRefById> GetEntityRefs(string entityName);

    object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> entity);
    void DeleteEntities(object[][] entityIdsToDelete);

  }

}
