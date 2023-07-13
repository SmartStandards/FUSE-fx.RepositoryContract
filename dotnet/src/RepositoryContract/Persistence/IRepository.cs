using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

//TODO: rename namespace of the final versions to "System.Data.Fuse"
namespace System.Data.Fuse {

  public interface IRepository<TEntity> where TEntity : class {

    IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> filter);

    IQueryable<TEntity> GetEntitiesDynamic(string dynamicLinqFilter);

    IList<Dictionary<string, object>> GetDtos();

    IList<EntityRefById> GetEntityRefs();

    TEntity AddOrUpdateEntity(Dictionary<string, JsonElement> entity);

    void DeleteEntities(JsonElement[][] entityIdsToDelete);

  }

}
