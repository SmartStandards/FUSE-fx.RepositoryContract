using System.Collections.Generic;
using System.Text.Json;

namespace System.Data.Fuse {

  public interface IRepository<TDbEntity> where TDbEntity : class {  

    IList<Dictionary<string, object>> GetBusinessModels();

    IList<EntityRefById> GetEntityRefs();

    TDbEntity AddOrUpdateEntity(Dictionary<string, JsonElement> businessModel);

    void DeleteEntities(JsonElement[][] entityIdsToDelete);

  }

}
